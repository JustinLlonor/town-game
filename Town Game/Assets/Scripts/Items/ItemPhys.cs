//using Photon.Pun;
//using Photon.Realtime;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using WebSocketSharp;

public class ItemPhys : MonoBehaviour
{
    public string itemName;
    public float interactTimer = .5f;
    public Color inspectionColor;
    public ItemData itemData = new ItemData();
    bool pickedUp = false;

    PlayerManager playerManager;
    //PhotonView view;
    ObjectManager om;
    InteractableFinder finder;
    Interactable interactable;
    Item item;

    private void Awake()
    {
        finder = FindObjectOfType<InteractableFinder>();
        //playerManager = FindObjectOfType<PlayerManager>();
        //view = gameObject.GetComponent<PhotonView>();
        om = FindObjectOfType<ObjectManager>();
        interactable = gameObject.GetComponent<Interactable>();
    }

    private void Start()
    {
        CreateItem();
        //view.RPC("CreateItem", RpcTarget.Others);
        //view.TransferOwnership(0);
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

    public void InspectItem()
    {
        interactable.hovers[1].interactKey = Interactable.InteractKey.None;
        interactable.hovers[1].color = inspectionColor;
        StartCoroutine(RollText(1, item.description));
        string fingerprintText = "";
        //if (itemData.fingerprints.Count == 0)
        //{
        //    fingerprintText = "Judging from the cleanliness, no one seems to have used it.";
        //} 
        //if (itemData.fingerprints.Count == 1)
        //{
        //    fingerprintText = "There are a visible set of fingerprints on the object.";
        //}
        //if (itemData.fingerprints.Count > 1)
        //{
        //    fingerprintText = "There seem to be many different smudges and scratches on the object.";
        //}
        StartCoroutine(RollText(2, fingerprintText));

    }

    public void PickUpItem()
    {
        if (pickedUp) return;
        PlayerInventory inventory = playerManager.currentPlayer.GetComponent<PlayerInventory>();
        string eName = inventory.hotbar[inventory.equippedSlot].ToString();
        //if (!eName.IsNullOrEmpty())
        //{
        //    Item item = om.itemSearch[inventory.hotbar[inventory.equippedSlot]];
        //    if (item.large)
        //    {
        //        finder.iValid = false;
        //        return;
        //    }
        //}
        //if (inventory.IsInventoryFull()) return;
        //int givenSlot = inventory.GiveItem(itemName, true);
        //if (givenSlot == -1) return;
        //inventory.CollectItemData(itemData, givenSlot);
        //view.TransferOwnership(PhotonNetwork.LocalPlayer);
        //view.RPC("RemoveItem", view.Owner);
        //pickedUp = true;
        //gameObject.GetComponent<MeshRenderer>().enabled = false;
        //gameObject.GetComponent<Interactable>().canInteract = false;
    }

    //[PunRPC]
    public void RemoveItem()
    {
        //PhotonNetwork.Destroy(gameObject);
    }

    //[PunRPC]
    public void CreateItem()
    {
        gameObject.GetComponent<Interactable>().hovers[0].lore = "Pick up";
        item = FindObjectOfType<ObjectManager>().itemSearch[itemName];
        gameObject.GetComponent<MeshFilter>().mesh = item.mesh;
        gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", item.texture);
        gameObject.GetComponent<MeshCollider>().sharedMesh = item.mesh;
    }

    //[PunRPC]
    public void SetName(string name)
    {
        itemName = name;
    }

    //[PunRPC]
    public void AddFingerprint()//Player player)
    {
        //itemData.fingerprints.Add(player);
    }

    //[PunRPC]
    public void AddMetadata(NetworkString<_4> key, int value)
    {
        itemData.metadata.Add(key, value);
    }

    IEnumerator RollText(int hoverIndex, string text, float characterDelay = .01f, float punctuationDelay = .1f)
    {
        string currentText = "";
        int i = 0;
        while (currentText.Length < text.Length)
        {
            char c = text[i];
            currentText += text[i];
            interactable.hovers[hoverIndex].lore = currentText;
            i++;
            if (c == ',')
            {
                yield return new WaitForSeconds(punctuationDelay);
            }
            else if (c == '.')
            {
                yield return new WaitForSeconds(punctuationDelay * 3f);
            } 
            else
            {
                yield return new WaitForSeconds(characterDelay);
            }
        }
    }
}
