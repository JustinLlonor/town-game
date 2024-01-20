using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPhys : MonoBehaviour
{
    public string itemName;
    public float interactTimer = .5f;

    PlayerManager playerManager;

    private void Awake()
    {
        playerManager = FindObjectOfType<PlayerManager>();
    }

    private void Start()
    {
        gameObject.GetComponent<Interactable>().hovers[0].lore = "Pick up " + itemName;
        Item item = FindObjectOfType<ObjectManager>().itemSearch[itemName];
        gameObject.GetComponent<MeshFilter>().mesh = item.mesh;
        gameObject.GetComponent<MeshRenderer>().material.SetTexture("_Texture", item.material.mainTexture);
        gameObject.GetComponent<MeshCollider>().sharedMesh = item.mesh;

        gameObject.GetComponent<PhotonView>().TransferOwnership(0);
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
        PlayerInventory inventory = playerManager.currentPlayer.GetComponent<PlayerInventory>();
        if (inventory.IsInventoryFull()) return;
        inventory.GiveItem(itemName, true);
        PhotonNetwork.Destroy(gameObject);
    }
}
