using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using UnityEngine.UI;
using Fusion;

// Sync player inventory stuff
public class PlayerInventory : NetworkBehaviour//PunCallbacks, IPunObservable
{
    [Networked] public int equippedSlot { get; set; } // To be synced, along with item show functions
    [Networked] public bool canSwitchSlots { get; set; } = true;
    [Networked] bool reequipTick { get; set; }
    bool previousReequip;
    [Header("Hotbar/Armor")]
    public int hotbarLength = 4;
    public ClothingGroup[] armorClothingGroups;
    /// <summary>
    /// The list of item names representing the player's inventory. Indices below hotbarLength represent the hotbar, and indices above
    /// hotbarLength represent armor.
    /// </summary>
    [Networked, Capacity(7)]public NetworkLinkedList<NetworkString<_32>> items { get; }
    [Networked, Capacity(7)]public NetworkLinkedList<ItemData> itemData { get; }// Item metadata
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
    public InventoryEvent OnSwitchSlot;

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
    private Material ogMaterial;
    List<int> equipLayers = new List<int>();
    int previousSlot;
    bool uiSetup = false;

    public delegate void InventoryEvent();
    ChangeDetector changeDetector;

    public void Init()
    {
        runnerManager = FindFirstObjectByType<RunnerManager>();
        camTransform = Camera.main.transform.parent;
        itemManager = FindFirstObjectByType<ObjectManager>();
        sFilter = sItem.GetComponent<MeshFilter>();
        sRenderer = sItem.GetComponent<MeshRenderer>();
        attackManager = gameObject.GetComponent<AttackManager>();
        rb = gameObject.GetComponent<Rigidbody>();
        mainCam = Camera.main.transform;
        fps = FindFirstObjectByType<FirstPerson>();
        ogMaterial = sRenderer.material;
    }

    private void Update()
    {
        Previous();
        if (!HasInputAuthority) return;
        //if (equippedItem != null) largeUI.SetActive(equippedItem.large);
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
            // Initialize inventory
            for (int i = 0; i < items.Capacity; i++)
            {
                items.Add("");
                itemData.Add(new ItemData());
            }
        }

        if (!HasInputAuthority) return;
        InputManager inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onEquipItem += OnEquipItem;
        //for (int i = 0; i < itemData.Length; i++) itemData[i] = null; item data stuff doesn't matter until an item enters that slot
    }

    //TODO: Hide hotbar from others, networked variable of the shown item
    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(equippedSlot):
                    if (!IsProxy) return;
                    if (items[equippedSlot].ToString().IsNullOrEmpty())
                    {
                        HideItem();
                        return;
                    }
                    ShowItem(items[equippedSlot].ToString());
                    break;
                case nameof(items):
                    if (HasInputAuthority) UpdateHotbarUI(true);
                    if (items[equippedSlot].ToString().IsNullOrEmpty())
                    {
                        HideItem();
                        return;
                    }
                    break;
            }
        }
    }

    public void Setup()
    {
        uiSetup = true;
        SetupHotbarUI();
        //UpdateHotbarUI();
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

    /// <summary>
    /// Creates hotbar UI
    /// </summary>
    void SetupHotbarUI()
    {
        for (int i = 0; i < hotbarLength; i++)
        {
            GameObject slotObject = Instantiate(hotbarSlot, hotbarUI);
            //SlotUI slotUI = slotObject.GetComponent<SlotUI>();
            //slotUI.SetIndex(i + 1);
        }
        hotbarUI.anchoredPosition -= new Vector2((hotbarLength - 1) * 50f, 0f); //For centering
    }

    /// <summary>
    /// Called whenever an item is unequipped
    /// </summary>
    /// <param name="previous"></param>
    private void OnUnequip(int previous) // To be set up
    {
        if (items.Count == 0) return;
        if (items[previous].ToString().IsNullOrEmpty()) return;
        if (itemManager.itemSearch[items[previous].ToString()] as Weapon)
        {
            
        }
    }

    public void UpdateHotbarUI(bool doEquip = true)
    {
        if (!uiSetup) return;
        for (int i = 0; i < hotbarLength; i++)
        {
            SlotAnimUI sAnimUI = hotbarUI.GetChild(i).GetComponent<SlotAnimUI>();
            SlotUI slotUI = hotbarUI.GetChild(i).GetChild(0).GetComponent<SlotUI>();
            //Sets icons
            if (items[i].ToString().IsNullOrEmpty())
            {
                slotUI.SetIcon(null);
            }
            else
            {
                slotUI.SetIcon(itemManager.itemSearch[items[i].ToString()].icon);
            }
            if (doEquip) sAnimUI.SetEquipped(equippedSlot == i);
            //slotUI.SetEquipped(equippedSlot == i);
            /**
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
            **/
        }
    }

    private void OnEquipItem(int slot)
    {
        if (slot == 0) return;
        runnerManager.hotbarKey = slot;
    }

    /**
     - Sync HideItem and ShowItem across network
     - Make this function only callable on fixedupdatenetwork, itemcomponent object should sync across client and server
    **/
    public void EquipItem(int slot, bool selfEquip = false)
    {
        if (slot >= hotbarLength) return; // Return if it is out of bounds/the index reaches armor, since you can't equip armor
        if (equippedSlot != slot) OnSwitchSlot?.Invoke();
        if (equippedSlot == slot && !selfEquip) return; // If the player equips the same slot they are holding?
        if (HasInputAuthority)
        {
            CrosshairManager.instance.RemoveCrosshair(1);
            if (largeUI != null) largeUI.SetActive(false);
        }
        if (itemComponentObject != null)
        {
            itemComponentObject.SendMessage("Deinitialize", SendMessageOptions.DontRequireReceiver);
            Destroy(itemComponentObject);
        }
        itemComponentObject = null;

        equippedSlot = slot;

        if (items[equippedSlot].ToString().IsNullOrEmpty()) // If the slot is empty, hide the item and return
        {
            equippedItem = null;
            HideItem(); // Sync with change detector
            return;
        }

        equippedItem = itemManager.itemSearch[items[equippedSlot].ToString()];
        if (equippedItem.itemBehaviourObject != null)
        {
            itemComponentObject = Instantiate(equippedItem.itemBehaviourObject, itemComponentHolder);
            if (equippedItem as Device)
            {
                itemComponentObject.AddComponent<DevicePlacement>();
            }
            Debug.Log("Initializing regular item");
            itemComponentObject.SendMessage("Initialize", 
                new ItemInitInfo(gameObject, itemData[equippedSlot].metadata, items[equippedSlot].ToString()), 
                SendMessageOptions.DontRequireReceiver); // Gives metadata information to any listeners
        }
        else
        {
            if (equippedItem as Device)
            {
                Debug.Log("initializing device");
                itemComponentObject = new GameObject("Device Placement", typeof(DevicePlacement));
                itemComponentObject.transform.parent = itemComponentHolder;
                itemComponentObject.SendMessage("Initialize", 
                    new ItemInitInfo(gameObject, itemData[equippedSlot].metadata, items[equippedSlot].ToString()), 
                    SendMessageOptions.DontRequireReceiver); // Gives metadata information to any listeners
            }
        }

        ShowItem(items[equippedSlot].ToString()); // Sync with change detector
    }

    // Shows the item on both client and server side
    public void ShowItem(string itemName)
    {
        if (itemName.IsNullOrEmpty()) return;
        Item equippedItem = itemManager.itemSearch[itemName];
        sFilter.mesh = equippedItem.mesh;
        if (equippedItem.material == null)
        {
            sRenderer.material = ogMaterial;
            sRenderer.material.SetTexture("_MainTex", equippedItem.texture);
        }
        else sRenderer.material = equippedItem.material;
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_ShowItem()
    {
        if (!IsProxy) return;
        ShowItem(items[equippedSlot].ToString());
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
        for (int i = 0; i < hotbarLength; i++)
        {
            if (items[i].ToString().IsNullOrEmpty())
            {
                return false;
            }
        }
        return true;
    }
    public bool IsInventoryFull(out int emptySlot) // Outputs the nearest empty slot
    {
        for (int i = 0; i < hotbarLength; i++)
        {
            if (items[i].ToString().IsNullOrEmpty())
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
    /// <param name="itemName">Name of the item</param>
    /// <param name="equipItem">If the player should equip it upon receiving</param>
    /// <param name="data">The data of the item, if applicable</param>
    /// <param name="slot"></param>
    /// <returns></returns>
    public int GiveItem(string itemName, bool equipItem = false, ItemData? data = null, int slot = -1)
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
        if (!items[slot].ToString().IsNullOrEmpty()) return -1; // Full inventory
        if (itemManager.itemSearch.ContainsKey(itemName))
        {
            items.Set(slot, itemName);
            if (equipItem) EquipItem(slot, slot == equippedSlot);
            if (!equipItem) EquipItem(equippedSlot, slot == equippedSlot);
            if (Runner.IsServer) reequipTick = !reequipTick;
        }
        // Get the item data
        if (data != null) SetItemMetadata((ItemData)data, slot);
        if (HasInputAuthority) UpdateHotbarUI();
        RPC_ShowItem();
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
            for (int i = 0; i < hotbarLength; i++)
            {
                if (items[i] == itemName)
                {
                    items.Set(i, "");
                    EquipItem(equippedSlot, i == equippedSlot);
                    break;
                }
            }
        }
        else
        {
            if (items[slot] == itemName)
            {
                items.Set(slot, "");
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
        if (items[slot].ToString().IsNullOrEmpty()) return;
        items.Set(slot, "");
        ClearItemMetadata(slot);
        EquipItem(equippedSlot, slot == equippedSlot);
        if (Runner.IsServer) reequipTick = !reequipTick;

        if (HasInputAuthority) UpdateHotbarUI();
    }

    /// <summary>
    /// Determines if two slots are capable of swapping, works on client and server
    /// </summary>
    /// <param name="slot1"></param>
    /// <param name="slot2"></param>
    /// <returns></returns>
    public bool CanSwap(int slot1, int slot2)
    {
        if (slot1 >= 0 && slot2 >= 0) return true; // both are in hotbar, will always be able to swap
        if (slot1 < 0 && slot2 < 0) return false; // both are attire, will not be able to swap
        // From this point, one is clothing slot the other is an item slot
        // Sets the relevant clothing group
        ClothingGroup clothingGroup;
        int itemSlot;
        if (slot1 < 0)
        {
            clothingGroup = GetClothingGroup(slot1);
            itemSlot = slot2;
        }
        else
        {
            clothingGroup = GetClothingGroup(slot2);
            itemSlot = slot1;
        }
        // Check if the item can get put in the armor slot
        Item itemItem = GetItemAtSlot(itemSlot);
        if (itemItem != null)
        {
            if (!(itemItem as Armor))
            {
                return false; // if the item is not armor, then you cannot swap
            }
            // Assuming it is armor
            Armor itemItemArmor = (Armor)itemItem;
            if (itemItemArmor.clothingGroup == clothingGroup) return true;
            return false; // if an armor within group, return true, otherwise return false
        }
        return true; // By default return true, if the item is null
    }

    /// <summary>
    /// Swaps items in 2 slots
    /// </summary>
    /// <param name="slot1"></param>
    /// <param name="slot2"></param>
    public void SwapItems(int slot1, int slot2)
    {
        if (slot1 == slot2) return;
        if (!CanSwap(slot1, slot2)) return;
        int slot1Index = GetArmorSlotIndex(slot1);
        int slot2Index = GetArmorSlotIndex(slot2);
        bool slot1Empty = items[slot1Index].ToString().IsNullOrEmpty();
        bool slot2Empty = items[slot2Index].ToString().IsNullOrEmpty();
        if (slot1Empty && slot2Empty) return; // Both empty, return
        if (equippedSlot == slot1Index || equippedSlot == slot2Index)
        {
            reequipTick = !reequipTick;
        }
        // If only one of the slots is empty
        if (slot1Empty)
        {
            MoveSlotToEmpty(slot1Index, slot2Index);
            return;
        }
        if (slot2Empty)
        {
            MoveSlotToEmpty(slot2Index, slot1Index);
            return;
        }
        // Swapping code
        NetworkString<_32> slot1Name = items[slot1Index];
        ItemData slot1Data = itemData[slot1Index];
        items.Set(slot1Index, items[slot2Index]);
        SetItemMetadata(itemData[slot2Index], slot1Index);
        items.Set(slot2Index, slot1Name);
        SetItemMetadata(slot1Data, slot2Index);
    }

    /// <summary>
    /// Moves an item slot to an empty slot.
    /// </summary>
    /// <param name="emptySlot"></param>
    /// <param name="itemSlot"></param>
    private void MoveSlotToEmpty(int emptySlot, int itemSlot)
    {
        items.Set(emptySlot, items[itemSlot]);
        SetItemMetadata(itemData[itemSlot], emptySlot);
        items.Set(itemSlot, "");
        ClearItemMetadata(itemSlot);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SendSwap(int slot1, int slot2)
    {
        SwapItems(slot1, slot2);
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

    private void SetItemMetadata(ItemData data, int itemIndex)
    {
        itemData.Set(itemIndex, new ItemData(data.metadata, data.fingerprints));
    }

    private void ClearItemMetadata(int itemIndex)
    {
        itemData.Set(itemIndex, new ItemData());
    }

    public Item GetHeldItem()
    {
        string itemName = items[equippedSlot].ToString();
        if (itemName.IsNullOrEmpty()) return null;
        return ObjectManager.i.itemSearch[itemName];
    }

    public ItemData GetHeldItemData()
    {
        return itemData[equippedSlot];
    }

    /// <summary>
    /// Gets the item at the slot. Negative values mean attire.
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public Item GetItemAtSlot(int slot)
    {
        // Returns the armor, if the slot is a negative indice
        if (slot < 0)
        {
            int armorSlot = GetArmorSlotIndex(slot);
            string armorName = items[armorSlot].ToString();
            if (armorName.IsNullOrEmpty()) return null;
            return ObjectManager.i.itemSearch[armorName];
        }
        // Returns an item within the hotbar if it is positive
        string itemName = items[slot].ToString();
        if (itemName.IsNullOrEmpty()) return null;
        return ObjectManager.i.itemSearch[itemName];
    }

    /// <summary>
    /// If the slot is less than zero, then this will turn it into the corresponding armor slot index 
    /// within the items list.
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    private int GetArmorSlotIndex(int slot)
    {
        if (slot >= 0) return slot;
        int armorSlot = hotbarLength + Mathf.Abs(slot) - 1;
        return armorSlot;
    }

    private ClothingGroup GetClothingGroup(int slot)
    {
        if (slot >= 0) return ClothingGroup.None;
        ClothingGroup output = armorClothingGroups[Mathf.Abs(slot) - 1];
        return output;
    }
}
