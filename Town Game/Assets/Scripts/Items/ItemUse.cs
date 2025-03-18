using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using UnityEngine.EventSystems;
using Fusion;

public class ItemUse : NetworkBehaviour
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public PlayerInventory inventory;
    [HideInInspector] public AttackManager attackManager;
    ObjectManager itemManager;
    //PhotonView view;
    CursorManager cm;
    RunnerManager runnerManager;

    private void Awake()
    {
        cm = FindFirstObjectByType<CursorManager>();
        //view = gameObject.GetComponent<PhotonView>();
        itemManager = FindFirstObjectByType<ObjectManager>();
    }

    private void Update()
    {
        // Update to new input system later
        //if (!view.IsMine) return;
        if (!cm.isLocked) return;
    }

    public override void Spawned()
    {
        if (!HasInputAuthority) return;
        InputManager inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onPrimaryFire += OnPrimaryItem;
        inputManager.onSecondaryFire += OnSecondaryItem;
        runnerManager = FindFirstObjectByType<RunnerManager>();
    }

    private void OnPrimaryItem()
    {
        runnerManager.itemUse = true;
    }

    private void OnSecondaryItem()
    {
        runnerManager.itemUseSecondary = true;
    }
    
    public void UseItem()
    {
        if (inventory.hotbar[inventory.equippedSlot].ToString().IsNullOrEmpty()) return;
        Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot].ToString()];
        if (item as Weapon)
        {
            Weapon weapon = (Weapon)item;
            attackManager.Attack(weapon); // called on client and server
            return;
        }
        if (item == null) return; // If item doesn't exist

        inventory.itemComponentObject.SendMessage("OnPrimaryUse", SendMessageOptions.DontRequireReceiver); // Sends the message OnPrimaryUse to every component in the item component holder
    }

    public void UseSecondary()
    {
        if (inventory.hotbar[inventory.equippedSlot].ToString().IsNullOrEmpty()) return;
        Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot].ToString()];
        if (item == null) return;

        inventory.itemComponentObject.SendMessage("OnSecondaryUse", SendMessageOptions.DontRequireReceiver);
    }
}
