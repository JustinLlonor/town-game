//using Photon.Pun;
//using Photon.Realtime;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp;

public class ItemPhys : NetworkBehaviour
{
    [Networked] public NetworkString<_32> itemName { get; set; }
    [Networked] public ItemData itemData { get; set; } = new ItemData();
    public float interactTimer = .5f;
    public Color inspectionColor;
    [Networked] public bool pickedUp { get; set; }
    [Networked] public PlayerRef pickedPlayer { get; set; }

    PlayerManager playerManager;
    //PhotonView view;
    ObjectManager om;
    InteractableFinder finder;
    Interactable interactable;
    Item item;

    ChangeDetector changeDetector;

    private void Awake()
    {
        finder = FindFirstObjectByType<InteractableFinder>();
        playerManager = FindFirstObjectByType<PlayerManager>();
        //view = gameObject.GetComponent<PhotonView>();
        om = FindFirstObjectByType<ObjectManager>();
        interactable = gameObject.GetComponent<Interactable>();
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

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (!itemName.ToString().IsNullOrEmpty())
        {
            CreateItem();
            RenderItem();
        }
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case (nameof(itemName)):
                    CreateItem();
                    RenderItem();
                    break;
            }
                
        }

        if (HasInputAuthority)
        {
            interactable.canInteract = !pickedUp;
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
        //StartCoroutine(RollText(2, fingerprintText));

    }

    public void PickUpItem(PlayerRef player)
    {
        if (pickedUp) return;
        PlayerInventory inventory = playerManager.GetPlayerNetworkObject(player).GetComponent<PlayerInventory>();
        if (inventory == null) return;
        string eName = inventory.hotbar[inventory.equippedSlot].ToString();
        if (!eName.IsNullOrEmpty())
        {
            Item item = om.itemSearch[inventory.hotbar[inventory.equippedSlot].ToString()];
            if (item.large)
            {
                finder.iValid = false;
                return;
            }
        }
        int givenSlot = inventory.GiveItem(itemName.ToString(), true);
        if (givenSlot == -1) return; // If inventory is full, return
        inventory.CollectItemData(itemData, givenSlot);
        pickedUp = true;
        pickedPlayer = player;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<Interactable>().canInteract = false;
        if (HasStateAuthority) Runner.Despawn(Object);
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
        item = FindFirstObjectByType<ObjectManager>().itemSearch[itemName.ToString()];
    }

    public void RenderItem()
    {
        if (item == null) return;
        gameObject.GetComponent<MeshFilter>().mesh = item.mesh;
        gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", item.texture);
        SetColliderBounds();
    }

    void SetColliderBounds()
    {
        Bounds meshBounds = gameObject.GetComponent<MeshRenderer>().localBounds;
        BoxCollider itemCollider = gameObject.GetComponent<BoxCollider>();
        itemCollider.center = meshBounds.center;
        itemCollider.size = meshBounds.size;
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
