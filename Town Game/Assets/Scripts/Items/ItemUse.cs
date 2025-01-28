using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using UnityEngine.EventSystems;

public class ItemUse : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode useKey;
    public KeyCode secondaryUseKey;

    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayerInventory inventory;
    [HideInInspector] public AttackManager attackManager;
    ObjectManager itemManager;
    //PhotonView view;
    CursorManager cm;

    private void Awake()
    {
        cm = FindObjectOfType<CursorManager>();
        //view = gameObject.GetComponent<PhotonView>();
        itemManager = FindObjectOfType<ObjectManager>();
    }

    private void Update()
    {
        // Update to new input system later
        //if (!view.IsMine) return;
        if (!cm.isLocked) return;
        if (Input.GetKey(useKey))
        {
            UseItem();
        }
        if (Input.GetKey(secondaryUseKey))
        {
            UseSecondary();
        }
    }

    void UseItem()
    {
        if (inventory.hotbar[inventory.equippedSlot].IsNullOrEmpty()) return;
        Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot]];
        if (item as Weapon)
        {
            Weapon weapon = (Weapon)item;
            attackManager.Attack(weapon);
            return;
        }
        if (item == null) return; // If item doesn't exist

        inventory.itemComponentObject.SendMessage("OnPrimaryUse", SendMessageOptions.DontRequireReceiver); // Sends the message OnPrimaryUse to every component in the item component holder
    }

    void UseSecondary()
    {
        if (inventory.hotbar[inventory.equippedSlot].IsNullOrEmpty()) return;
        Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot]];
        if (item == null) return;

        inventory.itemComponentObject.SendMessage("OnSecondaryUse", SendMessageOptions.DontRequireReceiver);
    }
}
