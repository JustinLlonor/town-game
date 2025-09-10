//using Photon.Pun;
//using Photon.Realtime;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [Networked] public NetworkString<_32> room { get; set; } // The name of the MapRoom this item belongs to
    public ItemUIInfo iuii;
    public InteractableSettings pickUpSettings = new InteractableSettings();
    public InteractableSettings notOwnedSettings = new InteractableSettings();
    public InteractableSettings stealSettings = new InteractableSettings();
    public Material itemMaterial;

    PositionManager positionManager;
    RoomManager roomManager;
    PlayerManager playerManager;
    ObjectManager om;
    InteractableFinder finder;
    Interactable interactable;
    GameManager gameManager;
    Item item;
    bool init = false;
    bool inspecting = false;
    bool ownershipFound = false;
    [HideInInspector] public bool rolesRevealed = false;

    ChangeDetector changeDetector;
    public MapRoom mapRoom;

    [System.Serializable]
    public struct InteractableSettings
    {
        public string text;
        public bool enableKey;
        public Color textColor;
        public Color keyColor;
        public Color fillColor;
    }

    private void Awake()
    {
        finder = FindFirstObjectByType<InteractableFinder>();
        playerManager = FindFirstObjectByType<PlayerManager>();
        //view = gameObject.GetComponent<PhotonView>();
        om = FindFirstObjectByType<ObjectManager>();
        interactable = gameObject.GetComponent<Interactable>();
        interactable.onLook += SetInspect;
        positionManager = FindAnyObjectByType<PositionManager>();
        roomManager = FindAnyObjectByType<RoomManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        gameManager.OnRevealRoles += SetRevealRoles;
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
        if (init && !room.ToString().IsNullOrEmpty() && !ownershipFound && rolesRevealed)
        {
            mapRoom = roomManager.GetWorkBuilding(room.ToString());
            ownershipFound = true;
            mapRoom.onAccessUpdate += GetLocalOwnership;
            GetLocalOwnership();
        }
    }

    private void OnDisable()
    {
        if (ownershipFound)
        {
            if (mapRoom != null) mapRoom.onAccessUpdate -= GetLocalOwnership;
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
        init = true;
        GetComponent<ActionHolder>().GetAction("Pick up").onInteract += PickUpItem;
        GetComponent<ActionHolder>().GetAction("Inspect").onInteract += InspectItem;
    }

    public override void Render()
    {
        // Change detector for itme property here, then execute local
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

    public string GetOwnership()
    {
        return room.ToString();
    }

    private void SetInspect()
    {
        interactable.hovers[1].lore = "Inspect";
    }

    public void InspectItem(Player player)
    {
        iuii.DescriptionReveal();
    }

    public void PickUpItem(Player playerObject)
    {
        if (!Runner.IsServer) return;
        PlayerRef player = playerObject.owner;
        if (pickedUp) return;
        if (!PlayerCanPickUpItem(player)) return;
        PlayerInventory inventory = playerObject.GetComponent<PlayerInventory>();
        if (inventory == null) return;
        string eName = inventory.hotbar[inventory.equippedSlot].ToString();
        if (!eName.IsNullOrEmpty())
        {
            Item item = om.itemSearch[inventory.hotbar[inventory.equippedSlot].ToString()];
            /**
            if (item.large)
            {
                finder.iValid = false;
                return;
            }
            **/
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

    private bool PlayerCanPickUpItem(PlayerRef player)
    {
        // If the player is a cultist they can always pick up an item
        bool isCultist = playerManager.GetIsCultist(player);
        if (isCultist) return true;
        // If not a cultist, they need access to a room to be able to pick up the item
        if (positionManager.PlayerHasAccessToRoom(player, room.ToString())) return true;
        return false;
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
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        if (item.material == null)
        {
            renderer.material = itemMaterial;
            renderer.material.SetTexture("_MainTex", item.texture);
        }
        else renderer.material = item.material;
        SetColliderBounds();
    }

    private void GetLocalOwnership(PlayerRef player)
    {
        GetLocalOwnership();
    }

    public void GetLocalOwnership()
    {
        bool isCultist = playerManager.GetIsCultist(Runner.LocalPlayer);
        bool ownsProperty = positionManager.PlayerHasAccessToRoom(Runner.LocalPlayer, room.ToString());
        if (!isCultist)
        {
            if (ownsProperty)
            {
                SetPickupHover(pickUpSettings);
            }
            else
            {
                SetPickupHover(notOwnedSettings);
            }
        }
        else
        {
            if (ownsProperty)
            {
                SetPickupHover(pickUpSettings);
            }
            else
            {
                SetPickupHover(stealSettings);
            }
        }
    }

    void SetPickupHover(InteractableSettings settings)
    {
        Interactable.Hover hover = interactable.hovers[0];
        hover.lore = settings.text;
        hover.color = settings.textColor;
        hover.fillColor = settings.fillColor;
        hover.keyColor = settings.keyColor;
        if (settings.enableKey)
        {
            hover.interactKey = Interactable.InteractKey.Interact1;
            return;
        }
        hover.interactKey = Interactable.InteractKey.None;
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

    public void SetRevealRoles(bool role)
    {
        rolesRevealed = true;
        gameManager.OnRevealRoles -= SetRevealRoles;
    }
}
