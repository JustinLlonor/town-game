using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenuUI : MonoBehaviour
{
    public MenuSlotUI[] slotUIs;
    private Dictionary<int, MenuSlotUI> slotIndexes = new Dictionary<int, MenuSlotUI>();
    private int? hoveredSlot = null;
    private int? pickedSlot = null;
    public UIManager uiManager;
    public RawImage pickImage;
    InputManager inputManager;
    PlayerInventory inventory;

    private void Awake()
    {
        inputManager = FindAnyObjectByType<InputManager>();
        foreach (MenuSlotUI slotUI in slotUIs)
        {
            slotUI.onHoverChange += SlotHover;
            slotIndexes.Add(slotUI.slotID, slotUI);
        }
    }

    private void Update()
    {
        pickImage.transform.position = Input.mousePosition;
    }

    private void OnEnable()
    {
        if (uiManager.trackedPlayer == null) return;
        inventory = uiManager.trackedPlayer.GetComponent<PlayerInventory>();
        pickedSlot = null;
        DisplayInventory();
        SetPickImage(null);
        inputManager.onSlotSwitch += SlotClick;
        inputManager.onSwap1 += Swap1;
        inputManager.onSwap2 += Swap2;
        inputManager.onSwap3 += Swap3;
        inputManager.onSwap4 += Swap4;
    }

    private void OnDisable()
    {
        inputManager.onSlotSwitch -= SlotClick;
        inputManager.onSwap1 -= Swap1;
        inputManager.onSwap2 -= Swap2;
        inputManager.onSwap3 -= Swap3;
        inputManager.onSwap4 -= Swap4;
    }

    private void SlotHover(bool hovering, int slotID)
    {
        DisplayInventory();
        if (hovering)
        {
            hoveredSlot = slotID;
            return;
        }
        if (slotID == hoveredSlot)
        {
            hoveredSlot = null;
        }
    }

    private void SlotClick()
    {
        if (hoveredSlot == null) return; // Only activates when hovering over a slot
        // Set picked slot to hovered slot
        if (pickedSlot == null)
        {
            Item pickedItem = inventory.GetItemAtSlot((int)hoveredSlot);
            if (pickedItem == null) return; // return if we click on an empty slot
            pickedSlot = hoveredSlot;
            SetPickImage(pickedItem);
            slotIndexes[(int)pickedSlot].slotUI.SetIcon(null);
            return;
        }
        // Swap with hovered slot
        if (pickedSlot != hoveredSlot)
        {
            if (!inventory.CanSwap((int)pickedSlot, (int)hoveredSlot)) return;
            Swap((int)pickedSlot, true);
            pickedSlot = null;
            SetPickImage(null);
            return;
        }
        Item pItem = inventory.GetItemAtSlot((int)pickedSlot);
        slotIndexes[(int)pickedSlot].slotUI.SetIcon(pItem.icon);
        pickedSlot = null;
        SetPickImage(null);
    }

    private void SetPickImage(Item item)
    {
        Texture2D tex = null;
        if (item != null) tex = item.icon;
        if (tex == null)
        {
            pickImage.enabled = false;
            return;
        }
        pickImage.enabled = true;
        pickImage.texture = tex;
    }

    private void Swap1() { Swap(0); }
    private void Swap2() { Swap(1); }
    private void Swap3() { Swap(2); }
    private void Swap4() { Swap(3); }

    /// <summary>
    /// Swaps the input slot ID with the hovered slot
    /// </summary>
    /// <param name="slotID"></param>
    private void Swap(int slotID, bool pickedItem = false)
    {
        if (hoveredSlot == null) return;
        if (hoveredSlot == slotID) return;
        if (!inventory.CanSwap(slotID, (int)hoveredSlot)) return;
        VisualSwap(slotID, (int)hoveredSlot, pickedItem);
        inventory.RPC_SendSwap(slotID, (int)hoveredSlot);
    }

    /// <summary>
    /// Swaps the textures between two slots on the client
    /// </summary>
    /// <param name="slot1"></param>
    /// <param name="slot2"></param>
    private void VisualSwap(int slot1, int slot2, bool usePickedItem)
    {
        MenuSlotUI slotUI1 = slotIndexes[slot1];
        MenuSlotUI slotUI2 = slotIndexes[slot2];
        if (usePickedItem)
        {
            Item item1 = inventory.GetItemAtSlot(slot1);
            Item item2 = inventory.GetItemAtSlot(slot2);
            slotUI1.slotUI.SetIconItem(item2);
            slotUI2.slotUI.SetIconItem(item1);
            return;
        }
        bool slot1UIEnabled = slotUI1.slotUI.icon.enabled;
        bool slot2UIEnabled = slotUI2.slotUI.icon.enabled;
        Texture slot1Tex = slotUI1.slotUI.icon.texture;
        if (slot2UIEnabled) slotUI1.slotUI.SetIcon(slotUI2.slotUI.icon.texture);
        else slotUI1.slotUI.SetIcon(null);
        if (slot1UIEnabled) slotUI2.slotUI.SetIcon(slot1Tex);
        else slotUI2.slotUI.SetIcon(null);
    }

    private void DisplayInventory()
    {
        foreach (MenuSlotUI slotUI in slotUIs)
        {
            if (slotUI.slotID == pickedSlot)
            {
                slotUI.slotUI.SetIcon(null);
                continue;
            }
            Item foundItem = inventory.GetItemAtSlot(slotUI.slotID);
            if (foundItem == null)
            {
                slotUI.slotUI.SetIcon(null);
                continue;
            }
            slotUI.slotUI.SetIcon(foundItem.icon);
        }
    }
}
