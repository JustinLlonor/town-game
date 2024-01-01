using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;
using UnityEngine.Animations.Rigging;
using WebSocketSharp;

public class PlayerInventory : MonoBehaviourPunCallbacks, IPunObservable
{
    public int equippedSlot;
    public List<string> hotbar = new List<string>();
    public Transform sItem; // Server item
    public Transform cItem; // Client item
    public Transform itemHolder; // ^^
    public Transform camTransform;
    public GameObject hotbarSlot;
    public RectTransform hotbarUI;
    private PhotonView view;
    private float itemPull = 40f;
    private float itemDrag = 40f;
    private ItemManager itemManager;
    private MeshFilter cFilter;
    private MeshRenderer cRenderer;
    private MeshFilter sFilter;
    private MeshRenderer sRenderer;
    private Vector3 itemPosition; // Local position of client item
    private float yOffset;
    private KeyCode[] hotbarInput =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
    };

    private void Awake()
    {
        view = gameObject.GetComponent<PhotonView>();
        itemManager = FindObjectOfType<ItemManager>();
        sFilter = sItem.GetComponent<MeshFilter>();
        sRenderer = sItem.GetComponent<MeshRenderer>();
        cFilter = cItem.GetComponent<MeshFilter>();
        cRenderer = cItem.GetComponent<MeshRenderer>();
        itemPosition = cItem.localPosition;
    }

    private void Start()
    {
        if (!view.IsMine)
        {
            Destroy(itemHolder.gameObject);
        }
        itemHolder.parent = null;
        SetupHotbarUI();
        EquipItem(0);
    }

    private void Update()
    {
        if (!view.IsMine) return;
        HotbarControls();
    }

    private void LateUpdate()
    {
        FollowItemTarget();
    }

    void SetupHotbarUI()
    {
        for (int i = 0; i < hotbar.Count; i++)
        {
            Instantiate(hotbarSlot, hotbarUI);
        }
        hotbarUI.anchoredPosition -= new Vector2((hotbar.Count - 1) * 50f, 0f);
    }

    private void HotbarControls()
    {
        for(int i = 0; i < hotbarInput.Length; i++)
        {
            if (Input.GetKeyDown(hotbarInput[i]))
            {
                if (i <  hotbar.Count)
                {
                    EquipItem(i);
                }
            }
        }
    }

    private void FollowItemTarget()
    {
        itemHolder.position = camTransform.position;
        if (cItem.localPosition != itemPosition)
        {
            cItem.localPosition = Vector3.Lerp(cItem.localPosition, new Vector3(itemPosition.x, yOffset, itemPosition.z), Time.deltaTime * itemPull);
        }
        if (cItem.localRotation != Quaternion.identity)
        {
            cItem.localRotation = Quaternion.Lerp(cItem.localRotation, Quaternion.identity, Time.deltaTime * itemPull);
        }
        itemHolder.rotation = Quaternion.Lerp(itemHolder.rotation, camTransform.rotation, Time.deltaTime * itemDrag);
    }

    public void EquipItem(int slot)
    {
        if (!view.IsMine) return;
        if (equippedSlot == slot) return;
        equippedSlot = slot;
        if (hotbar[equippedSlot].IsNullOrEmpty())
        {
            HideItem();
            view.RPC("HideItem", RpcTarget.OthersBuffered);
            return;
        }

        ShowItem(hotbar[equippedSlot]);
        view.RPC("ShowItem", RpcTarget.OthersBuffered, hotbar[equippedSlot]);
    }

    [PunRPC]
    public void ShowItem(string itemName)
    {
        Item equippedItem = itemManager.itemSearch[itemName];
        sFilter.mesh = equippedItem.model;
        sRenderer.material = equippedItem.material;
        // Client side
        if (!view.IsMine) return;
        cFilter.mesh = equippedItem.model;
        cRenderer.material = equippedItem.material;
        yOffset = equippedItem.yOffset;
        cItem.localPosition = itemPosition - (Vector3.down * 0.1f);
        cItem.localEulerAngles = new Vector3(30f, 0f, 0f);
        itemPull = equippedItem.pullSpeed;
        itemDrag = equippedItem.dragSpeed;
    }

    [PunRPC]
    public void HideItem()
    {
        sFilter.mesh = null;
        if (!view.IsMine) return;
        cFilter.mesh = null;
    }

    [PunRPC]
    public void AddItem(string itemName, int slot)
    {
        if (!hotbar[slot].IsNullOrEmpty()) return;
        if (itemManager.itemSearch.ContainsKey(itemName))
        {
            hotbar[slot] = itemName;
            equippedSlot = -1;
            EquipItem(slot);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
