using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
using WebSocketSharp;
using UnityEngine.UI;
using Fusion;
using UnityEngine.InputSystem;
using static Fusion.NetworkBehaviour;
using Unity.VisualScripting;
//using Photon.Realtime;

// Sync player inventory stuff
public class PlayerInventory : NetworkBehaviour//PunCallbacks, IPunObservable
{
    [Networked] public int equippedSlot { get; set; } // To be synced, along with item show functions
    [Networked] bool reequipTick { get; set; }
    bool previousReequip;
    [Header("Hotbar")]
    public int hotbarLength = 4;
    [Networked, Capacity(4)]public NetworkLinkedList<NetworkString<_32>> hotbar { get; }
    [Networked, Capacity(4)]public NetworkLinkedList<ItemData> itemData { get; }// Item metadata
    public GameObject hotbarSlot;
    public RectTransform hotbarUI;
    public GameObject largeUI;
    [Header("Item Display")]
    public Transform sItem; // Server item
    public Transform camTransform;
    public float dragMax = 5f;
    [Header("Item References")]
    public Animator animator;
    public Transform itemComponentHolder;
    [Header("Item Dropping")]
    public float dropVelocity = .5f;
    public float movementMultiplier = 2f;
    public float pickupCooldown = .5f;
    public GameObject itemPrefab;

    [HideInInspector] public GameObject itemComponentObject; // The GameObject that is a child of the physical item that contains item behaviours.
    FirstPerson fps;
    public Item equippedItem = null;
    AttackManager attackManager;
    //PhotonView view;
    ObjectManager itemManager;
    RunnerManager runnerManager;
    MeshFilter sFilter;
    MeshRenderer sRenderer;
    Transform mainCam;
    Rigidbody rb;
    List<int> equipLayers = new List<int>();
    int previousSlot;
    bool uiSetup = false;

    ChangeDetector changeDetector;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        runnerManager = FindObjectOfType<RunnerManager>();
        camTransform = Camera.main.transform.parent;
        itemManager = FindObjectOfType<ObjectManager>();
        sFilter = sItem.GetComponent<MeshFilter>();
        sRenderer = sItem.GetComponent<MeshRenderer>();
        attackManager = gameObject.GetComponent<AttackManager>();
        rb = gameObject.GetComponent<Rigidbody>();
        mainCam = Camera.main.transform;
        fps = FindObjectOfType<FirstPerson>();
    }

    private void Update()
    {
        Previous();
        if (!HasInputAuthority) return;
        if (equippedItem != null) largeUI.SetActive(equippedItem.large);
    }



    private void LateUpdate()
    {
        //if (!view.IsMine) return;
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        Init();
        if (Runner.IsServer)
        {
            for (int i = 0; i < hotbarLength; i++)
            {
                hotbar.Add("");
                itemData.Add(new ItemData());
            }
        }
        //for (int i = 0; i < itemData.Length; i++) itemData[i] = null; item data stuff doesn't matter until an item enters that slot
        if (!HasInputAuthority) return;
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(equippedSlot):
                    
                    break;
            }
        }
    }

    public void Setup()
    {
        uiSetup = true;
        SetupHotbarUI();
        UpdateHotbarUI();
    }

    public void Previous()
    {
        if (previousSlot != equippedSlot)
        {
            previousSlot = equippedSlot;
        }
        if (previousReequip != reequipTick)
        {
            if (HasInputAuthority)
            {
                EquipItem(equippedSlot, true);
                UpdateHotbarUI();
            }
            previousReequip = reequipTick;
        }
    }

    void SetupHotbarUI()
    {
        for (int i = 0; i < hotbarLength; i++)
        {
            Instantiate(hotbarSlot, hotbarUI);
        }
        hotbarUI.anchoredPosition -= new Vector2((hotbar.Count - 1) * 50f, 0f); //For centering
    }

    /// <summary>
    /// Called whenever an item is unequipped
    /// </summary>
    /// <param name="previous"></param>
    private void OnUnequip(int previous) // To be set up
    {
        if (hotbar.Count == 0) return;
        if (hotbar[previous].ToString().IsNullOrEmpty()) return;
        if (itemManager.itemSearch[hotbar[previous].ToString()] as Weapon)
        {
            attackManager.ResetAttack();
        }
    }

    public void UpdateHotbarUI()
    {
        if (!uiSetup) return;
    //    //if (!view.IsMine) return;
        for (int i = 0; i < hotbar.Count; i++)
        {
            RawImage panel = hotbarUI.GetChild(i).GetChild(0).GetComponent<RawImage>(); // WHAT THE FUCK
            RawImage icon = hotbarUI.GetChild(i).GetChild(0).GetChild(0).GetComponent<RawImage>();
            //Sets icons
            if (hotbar[i].ToString().IsNullOrEmpty())
            {
                icon.enabled = false;
            } 
            else
            {
                icon.enabled = true;
                icon.texture = itemManager.itemSearch[hotbar[i].ToString()].icon;
            }
            //Sets icon colors
            if (equippedSlot == i)
            {
                panel.color = new Color(1f, 1f, 1f, .37f);
            }
            else
            {
                panel.color = new Color(0f, 0f, 0f, .37f); ;
            }
        }
    }

    private void OnEquipItem(InputValue iv)
    {
        int slot = (int)iv.Get<float>();
        if (slot == 0) return;
        runnerManager.hotbarKey = slot;
    }

    /**
     - Sync HideItem and ShowItem across network
     - Make this function only callable on fixedupdatenetwork, itemcomponent object should sync across client and server
    **/
    public void EquipItem(int slot, bool selfEquip = false)
    {
        if (equippedSlot == slot && !selfEquip) return; // If the player equips the same slot they are holding?
        if (HasInputAuthority)
        {
            CrosshairManager.instance.RemoveCrosshair(1);
            if (largeUI != null) largeUI.SetActive(false);
        }
        if (itemComponentObject != null) Destroy(itemComponentObject);
        itemComponentObject = null;

        equippedSlot = slot;

        if (hotbar[equippedSlot].ToString().IsNullOrEmpty()) // If the slot is empty, hide the item and return
        {
            equippedItem = null;
            HideItem(); // Sync with change detector
            return;
        }

        equippedItem = itemManager.itemSearch[hotbar[equippedSlot].ToString()];
        itemComponentObject = Instantiate(equippedItem.itemComponentHolder, itemComponentHolder);
        itemComponentObject.SendMessage("OnReceiveMetadata", itemData[equippedSlot].metadata, SendMessageOptions.DontRequireReceiver); // Gives metadata information to any listeners

        if (equippedItem as Weapon)
        {
            //Weapon weapon = (Weapon)equippedItem;
            //attackManager.SetAttackCooldown(weapon.attackCooldown);
            //CrosshairManager.instance.AddCrosshair(1, 1);
        }

        ShowItem(hotbar[equippedSlot].ToString()); // Sync with change detector
    }

    // Shows the item on both client and server side
    public void ShowItem(string itemName)
    {
        Item equippedItem = itemManager.itemSearch[itemName];
        sFilter.mesh = equippedItem.mesh;
        sRenderer.material.SetTexture("_MainTex", equippedItem.texture);
        ResetEquipLayers();
        // Play all pose animations on the item
        foreach (Item.AnimationState pose in equippedItem.holdPoses)
        {
            int layer = animator.GetLayerIndex(pose.layer);
            animator.Play(pose.animation, layer);
            animator.SetLayerWeight(layer, 1f);
            equipLayers.Add(layer);
        }

        // Client side
        if (!HasInputAuthority) return;
        fps.ShowClientItem(equippedItem);
    }

    // Hides the item by disabling renderers
    public void HideItem()
    {
        sFilter.mesh = null;
        ResetEquipLayers();
        if (!HasInputAuthority) return;
        fps.HideClientItem();
    }

    // Resets animations for the character
    void ResetEquipLayers()
    {
        foreach (int layer in equipLayers)
        {
            animator.SetLayerWeight(layer, 0f);
        }
        equipLayers.Clear();
    }

    public bool IsInventoryFull()
    {
        for (int i = 0; i < hotbar.Count; i++)
        {
            if (hotbar[i].ToString().IsNullOrEmpty())
            {
                return false;
            }
        }
        return true;
    }
    public bool IsInventoryFull(out int emptySlot) // Outputs the nearest empty slot
    {
        for (int i = 0; i < hotbar.Count; i++)
        {
            if (hotbar[i].ToString().IsNullOrEmpty())
            {
                emptySlot = i;
                return false;
            }
        }
        emptySlot = -1;
        return true;
    }

    /// <summary>
    /// Gives an item to this player.
    /// </summary>
    /// <param name="itemName">Name of item</param>
    /// <param name="slot">Slot to put item in, automatically finds a slot by default</param>
    public int GiveItem(string itemName, bool equipItem = false, int slot = -1)
    {
        int emptySlot;
        if (IsInventoryFull(out emptySlot))
        {
            return -1;
        }
        if (slot == -1)
        {
            slot = emptySlot; // Given slot
        }
        if (!hotbar[slot].ToString().IsNullOrEmpty()) return -1;
        if (itemManager.itemSearch.ContainsKey(itemName))
        {
            hotbar.Set(slot, itemName);
            if (equipItem) EquipItem(slot, slot == equippedSlot);
            if (!equipItem) EquipItem(equippedSlot, slot == equippedSlot);
            if (Runner.IsServer) reequipTick = !reequipTick;
        }
        if (HasInputAuthority) UpdateHotbarUI();
        return slot;
    }

    /// <summary>
    /// Removes an item from this player.
    /// </summary>
    /// <param name="itemName">Name of item</param>
    /// <param name="slot">Slot to remove item from, finds the item automatically by default</param>
    //[PunRPC]        
    public void RemoveItem(string itemName, int slot = -1)
    {
        if (slot == -1)
        {
            for (int i = 0; i < hotbar.Count; i++)
            {
                if (hotbar[i] == itemName)
                {
                    hotbar.Set(i, "");
                    EquipItem(equippedSlot, i == equippedSlot);
                    break;
                }
            }
        }
        else
        {
            if (hotbar[slot] == itemName)
            {
                hotbar.Set(slot, "");
                EquipItem(equippedSlot, slot == equippedSlot);
            }
        }

        if (HasInputAuthority) UpdateHotbarUI();
    }

    /// <summary>
    /// Removes an item at a specified slot
    /// </summary>
    /// <param name="slot">Slot index</param>
    public void RemoveItem(int slot)
    {
        if (hotbar[slot].ToString().IsNullOrEmpty()) return;
        hotbar.Set(slot, "");
        EquipItem(equippedSlot, slot == equippedSlot);
        if (Runner.IsServer) reequipTick = !reequipTick;

        if (HasInputAuthority) UpdateHotbarUI();
    }

    public void DropItem(int itemIndex)
    {
        //Item item = itemManager.itemSearch[hotbar[itemIndex].ToString()];
        //if (item == null) return;
        //if (hotbar[equippedSlot].IsNullOrEmpty()) return;
        //GameObject itemObj = PhotonNetwork.Instantiate(itemPrefab.name, mainCam.position, mainCam.rotation);
        //itemObj.GetComponent<Interactable>().canInteract = false;
        //Vector3 velocityAdd = Vector3.ClampMagnitude(rb.velocity / movementMultiplier, 3f);
        //itemObj.GetComponent<Rigidbody>().velocity = mainCam.forward * dropVelocity + velocityAdd;
        //PhotonView itemView = itemObj.GetComponent<PhotonView>();
        //itemView.RPC("SetName", RpcTarget.All, item.name);
        //AddFingerprint(itemIndex);
        //TransferItemData(itemIndex, itemView);
        //ItemPhys itemPhys = itemObj.GetComponent<ItemPhys>();
        //itemPhys.interactTimer = pickupCooldown;

        //itemData[itemIndex] = null;
        //RemoveItem(hotbar[equippedSlot].ToString(), equippedSlot);
    }

    void AddFingerprint(int itemIndex)
    {
        //if (!itemData[itemIndex].fingerprints.Contains(view.Owner)) itemData[itemIndex].fingerprints.Add(view.Owner); // Photon syncing
    }

    public void CollectItemData(ItemData data, int itemIndex)
    {
        itemData.Set(itemIndex, new ItemData(data.metadata, data.fingerprints));
    }
}
