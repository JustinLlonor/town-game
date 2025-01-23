using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using WebSocketSharp;
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
        return; // Change later
        //if (inventory.hotbar[inventory.equippedSlot].IsNullOrEmpty()) return;
        Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot]];
        if (item as Weapon)
        {
            Weapon weapon = (Weapon)item;
            attackManager.Attack(weapon);
            return;
        }
        //if (inventory.hotbar[inventory.equippedSlot].IsNullOrEmpty()) return;
        //if (!item.useMethod.IsNullOrEmpty())
        //{
        //    Invoke(item.useMethod, 0f);
        //}
    }

    void UseSecondary()
    {
        //if (inventory.hotbar[inventory.equippedSlot].IsNullOrEmpty()) return;
        Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot]];
        if (item == null) return;
        //if (!item.secondaryUseMethod.IsNullOrEmpty())
        //{
        //    Invoke(item.secondaryUseMethod, 0f);
        //}
    }

    void Empty()
    {
        Debug.Log("Empty"); 
    }
}
