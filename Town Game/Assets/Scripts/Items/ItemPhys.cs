using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class ItemPhys : MonoBehaviour
{
    public string itemName;
    public float interactTimer = .5f;
    bool pickedUp = false;

    PlayerManager playerManager;
    PhotonView view;
    ObjectManager om;

    private void Awake()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        view = gameObject.GetComponent<PhotonView>();
        om = FindObjectOfType<ObjectManager>();
    }

    private void Start()
    {
        CreateItem();
        view.RPC("CreateItem", RpcTarget.Others);
        view.TransferOwnership(0);
    }

    private void Update()
    {
        if (interactTimer > 0f)
        {
            interactTimer -= Time.deltaTime;
            if (interactTimer < 0f)
            {
                gameObject.GetComponent<Interactable>().canInteract = true;
            }
        }
    }

    public void PickUpItem()
    {
        if (pickedUp) return;
        PlayerInventory inventory = playerManager.currentPlayer.GetComponent<PlayerInventory>();
        string eName = inventory.hotbar[inventory.equippedSlot];
        if (!eName.IsNullOrEmpty())
        {
            Item item = om.itemSearch[inventory.hotbar[inventory.equippedSlot]];
            if (item.large) return;
        }
        if (inventory.IsInventoryFull()) return;
        inventory.GiveItem(itemName, true);
        view.TransferOwnership(PhotonNetwork.LocalPlayer);
        view.RPC("RemoveItem", view.Owner);
        pickedUp = true;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<Interactable>().canInteract = false;
    }

    [PunRPC]
    public void RemoveItem()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    public void CreateItem()
    {
        gameObject.GetComponent<Interactable>().hovers[0].lore = "Pick up " + itemName;
        Item item = FindObjectOfType<ObjectManager>().itemSearch[itemName];
        gameObject.GetComponent<MeshFilter>().mesh = item.mesh;
        gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", item.texture);
        gameObject.GetComponent<MeshCollider>().sharedMesh = item.mesh;
    }

    [PunRPC]
    public void SetName(string name)
    {
        itemName = name;
    }
}
