using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using WebSocketSharp;
using UnityEngine.UI;
using Photon.Realtime;

public class PlayerInventory : MonoBehaviourPunCallbacks, IPunObservable
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
    [Header("Item Animations")]
    public Animator animator;
    [Header("Item Dropping")]
    public float dropVelocity = .5f;
    public float movementMultiplier = 2f;
    public float pickupCooldown = .5f;
    public GameObject itemPrefab;
    [Header("Keybinds")]
    public KeyCode dropKey;

    FirstPerson fps;
    Item equippedItem = null;
    AttackManager attackManager;
    PhotonView view;
    ObjectManager itemManager;
    MeshFilter sFilter;
    MeshRenderer sRenderer;
    Transform mainCam;
    Rigidbody rb;
    KeyCode[] hotbarInput =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
    };
    List<int> equipLayers = new List<int>();
    int previousSlot;

    private void Awake()
    {
        itemData = new ItemData[hotbar.Count];
        for (int i = 0; i < itemData.Length; i++) itemData[i] = null;
        camTransform = Camera.main.transform.parent;
        view = gameObject.GetComponent<PhotonView>();
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
        if (!view.IsMine) return;
        if (previousSlot != equippedSlot)
        {
            OnUnequip(previousSlot);
            previousSlot = equippedSlot;
        }
        HotbarControls();
        if (equippedItem != null) largeUI.SetActive(equippedItem.large);
    }

    private void LateUpdate()
    {
        if (!view.IsMine) return;
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
        //hotbarUI.anchoredPosition -= new Vector2((hotbar.Count - 1) * 50f, 0f); //For centering
    }

    void UpdateHotbarUI()
    {
        if (!view.IsMine) return;
        for (int i = 0; i < hotbar.Count; i++)
        {
            RawImage panel = hotbarUI.GetChild(i).GetChild(0).GetComponent<RawImage>();
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

    void HotbarControls()
    {
        if (!hotbar[equippedSlot].IsNullOrEmpty())
        {
            if (equippedItem.large) return;
        }
        for(int i = 0; i < hotbarInput.Length; i++)
        {
            if (Input.GetKeyDown(hotbarInput[i]))
            {
                if (i <  hotbar.Count)
                {
                    EquipItem(i);
                }
                UpdateHotbarUI();
            }
        }
    }

    public void EquipItem(int slot, bool selfEquip = false)
    {
        if (!view.IsMine) return;
        CrosshairManager.instance.RemoveCrosshair(1);
        largeUI.SetActive(false);
        if (equippedSlot == slot && !selfEquip) return;
        equippedSlot = slot;
        if (hotbar[equippedSlot].IsNullOrEmpty())
        {
            equippedItem = null;
            HideItem();
            view.RPC("HideItem", RpcTarget.OthersBuffered);
            return;
        }
        equippedItem = itemManager.itemSearch[hotbar[equippedSlot]];
        if (equippedItem as Weapon)
        {
            Weapon weapon = (Weapon)equippedItem;
            attackManager.SetAttackCooldown(weapon.attackCooldown);
            CrosshairManager.instance.AddCrosshair(1, 1);
        }

        ShowItem(hotbar[equippedSlot]);
        view.RPC("ShowItem", RpcTarget.OthersBuffered, hotbar[equippedSlot]);
    }

    [PunRPC]
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
        if (!view.IsMine) return;
        fps.ShowClientItem(equippedItem);

        return; 
    }

    [PunRPC]
    public void HideItem()
    {
        sFilter.mesh = null;
        ResetEquipLayers();
        if (!view.IsMine) return;
        fps.HideClientItem();
    }

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
    public bool IsInventoryFull(out int emptySlot)
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
    [PunRPC]
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
    [PunRPC]        
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
        if (hotbar[equippedSlot].IsNullOrEmpty()) return;
        GameObject itemObj = PhotonNetwork.Instantiate(itemPrefab.name, mainCam.position, mainCam.rotation);
        itemObj.GetComponent<Interactable>().canInteract = false;
        Vector3 velocityAdd = Vector3.ClampMagnitude(rb.velocity / movementMultiplier, 3f);
        itemObj.GetComponent<Rigidbody>().velocity = mainCam.forward * dropVelocity + velocityAdd;
        PhotonView itemView = itemObj.GetComponent<PhotonView>();
        itemView.RPC("SetName", RpcTarget.All, item.name);
        AddFingerprint(itemIndex);
        TransferItemData(itemIndex, itemView);
        ItemPhys itemPhys = itemObj.GetComponent<ItemPhys>();
        itemPhys.interactTimer = pickupCooldown;

        itemData[itemIndex] = null;
        RemoveItem(hotbar[equippedSlot], equippedSlot);
    }

    void AddFingerprint(int itemIndex)
    {
        if (!itemData[itemIndex].fingerprints.Contains(view.Owner)) itemData[itemIndex].fingerprints.Add(view.Owner);
    }

    void TransferItemData(int itemIndex, PhotonView itemView)
    {
        ItemData data = itemData[itemIndex];
        foreach (Player player in data.fingerprints)
        {
            itemView.RPC("AddFingerprint", RpcTarget.AllBuffered, player);
        }
        foreach (KeyValuePair<string, string> pair in data.metadata)
        {
            itemView.RPC("AddMetadata", RpcTarget.AllBuffered, pair.Key, pair.Value);
        }
    }

    public void CollectItemData(ItemData data, int itemIndex)
    {
        itemData[itemIndex] = new ItemData();
        itemData[itemIndex].metadata = data.metadata;
        itemData[itemIndex].fingerprints = data.fingerprints;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
