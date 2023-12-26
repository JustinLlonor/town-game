using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;
using UnityEngine.Animations.Rigging;

public class PlayerInventory : MonoBehaviourPunCallbacks, IPunObservable
{
    public Item equippedItem;
    public float itemLerp = 40f;
    public Item[] hotbar = new Item[5];
    public int equippedSlot;
    private PhotonView view;
    public Transform sItem; // Server item
    public Transform cItem; // Client item
    private ItemManager itemManager;
    private MeshFilter cFilter;
    private MeshRenderer cRenderer;
    private MeshFilter sFilter;
    private MeshRenderer sRenderer;
    private Transform itemTarget;

    private void Awake()
    {
        view = gameObject.GetComponent<PhotonView>();
        itemManager = FindObjectOfType<ItemManager>();
        sFilter = sItem.GetComponent<MeshFilter>();
        sRenderer = sItem.GetComponent<MeshRenderer>();
        cFilter = cItem.GetComponent<MeshFilter>();
        cRenderer = cItem.GetComponent<MeshRenderer>();
        itemTarget = Camera.main.transform.GetChild(0);
    }

    private void Start()
    {
        if (!view.IsMine)
        {
            Destroy(cItem.gameObject);
        }
    }

    private void Update()
    {
        if (!view.IsMine) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShowItem("Fire Axe");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            HideItem();
        }

        FollowItemTarget();
    }

    private void FollowItemTarget()
    {
        if (equippedItem == null) return;
        cItem.position = Vector3.Lerp(cItem.position, itemTarget.position, Time.deltaTime * itemLerp);
        cItem.rotation = Quaternion.Lerp(cItem.rotation, itemTarget.rotation, Time.deltaTime * itemLerp);
    }

    [PunRPC]
    public void ShowItem(string itemName)
    {
        equippedItem = itemManager.itemSearch[itemName];
        sFilter.mesh = equippedItem.model;
        sRenderer.material = equippedItem.material;
        cFilter.mesh = equippedItem.model;
        cRenderer.material = equippedItem.material;
        itemTarget.localPosition = new Vector3(itemTarget.localPosition.x, equippedItem.yOffset, itemTarget.localPosition.z);
        cItem.position = new Vector3(itemTarget.position.x, itemTarget.position.y - 0.1f, itemTarget.position.z);
        cItem.eulerAngles = new Vector3(itemTarget.eulerAngles.x + 30f, cItem.eulerAngles.y, cItem.eulerAngles.z);
    }

    [PunRPC]
    public void HideItem()
    {
        sFilter.mesh = null;
        cFilter.mesh = null;
        equippedItem = null;
    }

    [PunRPC]
    public void AddItem(string itemName)
    {

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
