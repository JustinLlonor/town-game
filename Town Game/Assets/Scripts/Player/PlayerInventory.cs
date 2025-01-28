using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
using WebSocketSharp;
using UnityEngine.UI;
using Fusion;
using UnityEngine.InputSystem;
//using Photon.Realtime;

// Sync player inventory stuff
public class PlayerInventory : NetworkBehaviour//PunCallbacks, IPunObservable
{
    [Header("Hotbar")]
    public int equippedSlot;
    public List<string> hotbar = new List<string>();
    public ItemData[] itemData;
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
    [Header("Keybinds")]
    public KeyCode dropKey;

    [HideInInspector] public GameObject itemComponentObject; // The GameObject that is a child of the physical item that contains item behaviours.
    FirstPerson fps;
    Item equippedItem = null;
    AttackManager attackManager;
    //PhotonView view;
    ObjectManager itemManager;
    MeshFilter sFilter;
    MeshRenderer sRenderer;
    Transform mainCam;
    Rigidbody rb;
    List<int> equipLayers = new List<int>();
    int previousSlot;

    private void Awake()
    {
        itemData = new ItemData[hotbar.Count];
        for (int i = 0; i < itemData.Length; i++) itemData[i] = null;
        camTransform = Camera.main.transform.parent;
        //view = gameObject.GetComponent<PhotonView>();
        itemManager = FindObjectOfType<ObjectManager>();
        sFilter = sItem.GetComponent<MeshFilter>();
        sRenderer = sItem.GetComponent<MeshRenderer>();
        attackManager = gameObject.GetComponent<AttackManager>();
        rb = gameObject.GetComponent<Rigidbody>();
        mainCam = Camera.main.transform;
        fps = FindObjectOfType<FirstPerson>();
    }

    private void Start()
    {
        SetupHotbarUI();
        EquipItem(0);
        UpdateHotbarUI();
    }

    private void Update()
    {
        //if (!view.IsMine) return;
        if (previousSlot != equippedSlot)
        {
            OnUnequip(previousSlot);
            previousSlot = equippedSlot;
        }
        if (equippedItem != null) largeUI.SetActive(equippedItem.large);
    }

    private void LateUpdate()
    {
        //if (!view.IsMine) return;
    }

    private void OnDropItem()
    {
        DropItem(equippedSlot);
    }

    /// <summary>
    /// Called whenever an item is unequipped
    /// </summary>
    /// <param name="previous"></param>
    private void OnUnequip(int previous)
    {
        if (hotbar[previous].IsNullOrEmpty()) return;
        if (itemManager.itemSearch[hotbar[previous]] as Weapon)
        {
            attackManager.ResetAttack();
        }
    }

    void SetupHotbarUI()
    {
        for (int i = 0; i < hotbar.Count; i++)
        {
            Instantiate(hotbarSlot, hotbarUI);
        }
        hotbarUI.anchoredPosition -= new Vector2((hotbar.Count - 1) * 50f, 0f); //For centering
    }

    void UpdateHotbarUI()
    {
    //    //if (!view.IsMine) return;
        for (int i = 0; i < hotbar.Count; i++)
        {
            RawImage panel = hotbarUI.GetChild(i).GetChild(0).GetComponent<RawImage>(); // WHAT THE FUCK
            RawImage icon = hotbarUI.GetChild(i).GetChild(0).GetChild(0).GetComponent<RawImage>();
            //Sets icons
            if (hotbar[i].IsNullOrEmpty())
            {
                icon.enabled = false;
            } 
            else
            {
                icon.enabled = true;
                icon.texture = itemManager.itemSearch[hotbar[i]].icon;
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
        if (!hotbar[equippedSlot].IsNullOrEmpty())
        {
            if (equippedItem.large) return;
        }
        EquipItem(slot-1);
        UpdateHotbarUI();

    }

    /**
     - Sync HideItem and ShowItem across network
     - Make this function only callable on fixedupdatenetwork, itemcomponent object should sync across client and server
    **/
    public void EquipItem(int slot, bool selfEquip = false)
    {
        //if (!view.IsMine) return;
        CrosshairManager.instance.RemoveCrosshair(1);
        largeUI.SetActive(false);
        if (equippedSlot == slot && !selfEquip) return; // If the player equips the same slot they are holding

        if (itemComponentObject != null) Destroy(itemComponentObject);
        itemComponentObject = null;

        equippedSlot = slot;
        if (hotbar[equippedSlot].IsNullOrEmpty()) // If the slot is empty, hide the item and return
        {
            equippedItem = null;
            HideItem();
        //    view.RPC("HideItem", RpcTarget.OthersBuffered);
            return;
        }

        equippedItem = itemManager.itemSearch[hotbar[equippedSlot]];
        itemComponentObject = Instantiate(equippedItem.itemComponentHolder, itemComponentHolder);
        itemComponentObject.SendMessage("OnReceiveMetadata", itemData[equippedSlot].metadata, SendMessageOptions.DontRequireReceiver); // Gives metadata information to any listeners

        if (equippedItem as Weapon)
        {
            Weapon weapon = (Weapon)equippedItem;
            attackManager.SetAttackCooldown(weapon.attackCooldown);
            CrosshairManager.instance.AddCrosshair(1, 1);
        }

        ShowItem(hotbar[equippedSlot]);
        //view.RPC("ShowItem", RpcTarget.OthersBuffered, hotbar[equippedSlot]);
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
        //if (!view.IsMine) return;
        fps.ShowClientItem(equippedItem);

        return; 
    }

    // Hides the item by disabling renderers
    public void HideItem()
    {
        sFilter.mesh = null;
        ResetEquipLayers();
        //if (!view.IsMine) return;
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
            if (hotbar[i].IsNullOrEmpty())
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
            if (hotbar[i].IsNullOrEmpty())
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
    //[PunRPC]
    public int GiveItem(string itemName, bool equipItem = false, int slot = -1)
    {
        int emptySlot;
        if (IsInventoryFull(out emptySlot))
        {
            Debug.LogError("Inventory is full!");
            return -1;
        }
        if (slot == -1)
        {
            slot = emptySlot;
        }
        if (!hotbar[slot].IsNullOrEmpty()) return -1;
        if (itemManager.itemSearch.ContainsKey(itemName))
        {
            hotbar[slot] = itemName;
            if (equipItem) EquipItem(slot, slot == equippedSlot);
            if (!equipItem) EquipItem(equippedSlot, slot == equippedSlot);
        }
        UpdateHotbarUI();
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
                    hotbar[i] = "";
                    EquipItem(equippedSlot, i == equippedSlot);
                    break;
                }
            }
        }
        else
        {
            if (hotbar[slot] == itemName)
            {
                hotbar[slot] = "";
                EquipItem(equippedSlot, slot == equippedSlot);
            }
        }
        UpdateHotbarUI();
    }

    public void DropItem(int itemIndex)
    {
        Item item = itemManager.itemSearch[hotbar[itemIndex]];
        if (item == null) return;
        //if (hotbar[equippedSlot].IsNullOrEmpty()) return;
        //GameObject itemObj = PhotonNetwork.Instantiate(itemPrefab.name, mainCam.position, mainCam.rotation);
        //itemObj.GetComponent<Interactable>().canInteract = false;
        Vector3 velocityAdd = Vector3.ClampMagnitude(rb.velocity / movementMultiplier, 3f);
        //itemObj.GetComponent<Rigidbody>().velocity = mainCam.forward * dropVelocity + velocityAdd;
        //PhotonView itemView = itemObj.GetComponent<PhotonView>();
        //itemView.RPC("SetName", RpcTarget.All, item.name);
        AddFingerprint(itemIndex);
        //TransferItemData(itemIndex, itemView);
        //ItemPhys itemPhys = itemObj.GetComponent<ItemPhys>();
        //itemPhys.interactTimer = pickupCooldown;

        itemData[itemIndex] = null;
        RemoveItem(hotbar[equippedSlot], equippedSlot);
    }

    void AddFingerprint(int itemIndex)
    {
        //if (!itemData[itemIndex].fingerprints.Contains(view.Owner)) itemData[itemIndex].fingerprints.Add(view.Owner); // Photon syncing
    }

    void TransferItemData(int itemIndex)//, PhotonView itemView)
    {
        ItemData data = itemData[itemIndex];
        foreach (PlayerRef player in data.fingerprints)
        {
            //itemView.RPC("AddFingerprint", RpcTarget.AllBuffered, player); // Photon syncing
        }
        foreach (KeyValuePair<string, string> pair in data.metadata)
        {
            //itemView.RPC("AddMetadata", RpcTarget.AllBuffered, pair.Key, pair.Value);
        }
    }

    public void CollectItemData(ItemData data, int itemIndex)
    {
        itemData[itemIndex] = new ItemData();
        itemData[itemIndex].metadata = data.metadata;
        itemData[itemIndex].fingerprints = data.fingerprints;
    }
}
