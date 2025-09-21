using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using UnityEngine.EventSystems;
using Fusion;
using UnityEngine.InputSystem;

/// <summary>
/// Item use code
/// </summary>
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

    private void OnPrimaryItem(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            runnerManager.itemUsePrimary = true;
            return;
        }
        runnerManager.itemUsePrimary = false;
    }

    private void OnSecondaryItem(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            runnerManager.itemUseSecondary = true;
            return;
        }
        runnerManager.itemUseSecondary = false;
    }
    
    public void UsePrimary()
    {
        Item item = itemManager.itemSearch[inventory.items[inventory.equippedSlot].ToString()];
        if (item == null) return; // If item doesn't exist
        if (item as Weapon)
        {
            Weapon weapon = (Weapon)item;
            attackManager.Attack(weapon); // called on client and server
            return;
        }
        if (inventory.itemComponentObject == null) return;
        inventory.itemComponentObject.SendMessage("OnPrimaryUse", SendMessageOptions.DontRequireReceiver); // Sends the message OnPrimaryUse to every component in the item component holder
    }

    public void HoldPrimary()
    {
        //Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot].ToString()];
        //if (item == null) return; // If item doesn't exist
        if (inventory.itemComponentObject == null) return;
        inventory.itemComponentObject.SendMessage("OnPrimaryHold", SendMessageOptions.DontRequireReceiver); 
    }

    public void ReleasePrimary()
    {
        //Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot].ToString()];
        //if (item == null) return; // If item doesn't exist
        if (inventory.itemComponentObject == null) return;
        inventory.itemComponentObject.SendMessage("OnPrimaryRelease", SendMessageOptions.DontRequireReceiver);
    }

    public void UseSecondary()
    {
        //Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot].ToString()];
        //if (item == null) return;
        if (inventory.itemComponentObject == null) return;
        inventory.itemComponentObject.SendMessage("OnSecondaryUse", SendMessageOptions.DontRequireReceiver);
    }

    public void HoldSecondary()
    {
        //Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot].ToString()];
        //if (item == null) return; // If item doesn't exist
        if (inventory.itemComponentObject == null) return;
        inventory.itemComponentObject.SendMessage("OnSecondaryHold", SendMessageOptions.DontRequireReceiver);
    }

    public void ReleaseSecondary()
    {
        //Item item = itemManager.itemSearch[inventory.hotbar[inventory.equippedSlot].ToString()];
        //if (item == null) return; // If item doesn't exist
        if (inventory.itemComponentObject == null) return;
        inventory.itemComponentObject.SendMessage("OnSecondaryRelease", SendMessageOptions.DontRequireReceiver);
    }
}
