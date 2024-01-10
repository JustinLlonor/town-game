using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;
using UnityEngine.Animations.Rigging;
using WebSocketSharp;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Hotbar")]
    public int equippedSlot;
    public List<string> hotbar = new List<string>();
    public GameObject hotbarSlot;
    public RectTransform hotbarUI;
    [Header("Item Display")]
    public Transform sItem; // Server item
    public Transform cItem; // Client item
    public Transform itemHolder; // ^^
    public Transform camTransform;
    public float dragMax = 5f;
    [Header("Item Animations")]
    public Animator animator;
    public int rightHandLayer;
    [Header("Arms")]
    public ArmsManager arms;
    public Animator armsAnimator;
    
    AttackManager attackManager;
    PhotonView view;
    float itemPull = 40f;
    float itemDrag = 40f;
    ItemManager itemManager;
    MeshFilter cFilter;
    MeshRenderer cRenderer;
    MeshFilter sFilter;
    MeshRenderer sRenderer;
    Vector3 itemPosition; // Local position of client item
    float yOffset;
    KeyCode[] hotbarInput =
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
    List<int> equipLayers = new List<int>();
    int previousSlot;

    private void Awake()
    {
        view = gameObject.GetComponent<PhotonView>();
        itemManager = FindObjectOfType<ItemManager>();
        sFilter = sItem.GetComponent<MeshFilter>();
        sRenderer = sItem.GetComponent<MeshRenderer>();
        cFilter = cItem.GetComponent<MeshFilter>();
        cRenderer = cItem.GetComponent<MeshRenderer>();
        itemPosition = cItem.localPosition;
        attackManager = gameObject.GetComponent<AttackManager>();
    }

    private void Start()
    {
        if (!view.IsMine)
        {
            Destroy(itemHolder.gameObject);
            Destroy(arms.gameObject);
        }
        itemHolder.parent = null;
        arms.transform.parent = null;
        arms.gameObject.SetActive(false);
        SetupHotbarUI();
        EquipItem(0);
        UpdateHotbarUI();
    }

    private void Update()
    {
        if (!view.IsMine) return;
        if (previousSlot != equippedSlot)
        {
            OnUnequip(previousSlot);
            previousSlot = equippedSlot;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GiveItem("Fire Axe");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            GiveItem("Test");
        }
        HotbarControls();
    }

    private void LateUpdate()
    {
        if (!view.IsMine) return;
        FollowItemTarget();
        arms.GrabItem();
    }

    /// <summary>
    /// Called whenever an item is unequipped
    /// </summary>
    /// <param name="previous"></param>
    private void OnUnequip(int previous)
    {
        if (hotbar[previous].IsNullOrEmpty()) return;
        if (itemManager.itemSearch[hotbar[previous]] as Weapon)
        {
            attackManager.ResetAttack();
        }
    }

    void SetupHotbarUI()
    {
        for (int i = 0; i < hotbar.Count; i++)
        {
            Instantiate(hotbarSlot, hotbarUI);
        }
        //hotbarUI.anchoredPosition -= new Vector2((hotbar.Count - 1) * 50f, 0f); //For centering
    }

    void UpdateHotbarUI()
    {
        if (!view.IsMine) return;
        for (int i = 0; i < hotbar.Count; i++)
        {
            RawImage panel = hotbarUI.GetChild(i).GetChild(0).GetComponent<RawImage>();
            RawImage icon = hotbarUI.GetChild(i).GetChild(0).GetChild(0).GetComponent<RawImage>();
            //Sets icons
            if (hotbar[i].IsNullOrEmpty())
            {
                icon.enabled = false;
            } 
            else
            {
                icon.enabled = true;
                icon.texture = itemManager.itemSearch[hotbar[i]].icon;
            }
            //Sets icon colors
            if (equippedSlot == i)
            {
                panel.color = new Color(1f, 1f, 1f, .37f);
            }
            else
            {
                panel.color = new Color(0f, 0f, 0f, .37f); ;
            }
        }
    }

    void HotbarControls()
    {
        for(int i = 0; i < hotbarInput.Length; i++)
        {
            if (Input.GetKeyDown(hotbarInput[i]))
            {
                if (i <  hotbar.Count)
                {
                    EquipItem(i);
                }
                UpdateHotbarUI();
            }
        }
    }

    void FollowItemTarget()
    {
        itemHolder.position = camTransform.position;
        if (cItem.localRotation != Quaternion.identity)
        {
            cItem.localRotation = Quaternion.Lerp(cItem.localRotation, Quaternion.identity, Time.deltaTime * itemPull);
        }
        Quaternion newRot = Quaternion.Lerp(itemHolder.rotation, camTransform.rotation, Time.deltaTime * itemDrag);
        if (Quaternion.Angle(newRot, camTransform.rotation) > dragMax)
        {
            Vector2 capped = (itemHolder.eulerAngles - camTransform.eulerAngles).normalized * dragMax;
            itemHolder.eulerAngles = camTransform.eulerAngles + (Vector3)capped;
        }
        else
        {
            itemHolder.rotation = newRot;
        }
        if (attackManager.isAttacking) return;
        if (cItem.localPosition != itemPosition)
        {
            cItem.localPosition = Vector3.Lerp(cItem.localPosition, new Vector3(itemPosition.x, yOffset, itemPosition.z), Time.deltaTime * itemPull);
        }
    }

    public void EquipItem(int slot, bool selfEquip = false)
    {
        if (!view.IsMine) return;
        if (equippedSlot == slot && !selfEquip) return;
        equippedSlot = slot;
        if (hotbar[equippedSlot].IsNullOrEmpty())
        {
            HideItem();
            view.RPC("HideItem", RpcTarget.OthersBuffered);
            return;
        }
        if (itemManager.itemSearch[hotbar[equippedSlot]] as Weapon)
        {
            Weapon weapon = (Weapon)itemManager.itemSearch[hotbar[equippedSlot]];
            attackManager.SetAttackCooldown(weapon.attackCooldown);
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
        ResetEquipLayers();
        // Play all pose animations on the item
        foreach (Item.AnimationState pose in equippedItem.holdPoses)
        {
            int layer = animator.GetLayerIndex(pose.layer);
            animator.Play(pose.animation, layer);
            animator.SetLayerWeight(layer, 1f);
            equipLayers.Add(layer);
        }
        // Client side
        if (!view.IsMine) return;
        arms.gameObject.SetActive(true);
        cFilter.mesh = equippedItem.model;
        cRenderer.material = equippedItem.material;
        yOffset = equippedItem.yOffset;
        cItem.localPosition = itemPosition + (Vector3.down * equippedItem.iYOffset);
        cItem.localEulerAngles = new Vector3(equippedItem.angleOffset, 0f, 0f);
        itemPull = equippedItem.pullSpeed;
        itemDrag = equippedItem.dragSpeed;
        foreach (Item.AnimationState pose in equippedItem.holdPoses)
        {
            int layer = armsAnimator.GetLayerIndex(pose.layer);
            armsAnimator.Play(pose.animation, layer);
            armsAnimator.SetLayerWeight(layer, 1f);
        }
    }

    [PunRPC]
    public void HideItem()
    {
        sFilter.mesh = null;
        ResetEquipLayers();
        if (!view.IsMine) return;
        arms.gameObject.SetActive(false);
        cFilter.mesh = null;
    }

    void ResetEquipLayers()
    {
        foreach (int layer in equipLayers)
        {
            animator.SetLayerWeight(layer, 0f);
        }
        equipLayers.Clear();
    }

    /// <summary>
    /// Gives an item to this player.
    /// </summary>
    /// <param name="itemName">Name of item</param>
    /// <param name="slot">Slot to put item in, automatically finds a slot by default</param>
    [PunRPC]
    public void GiveItem(string itemName, int slot = -1)
    {
        if (slot == -1)
        {
            for (int i = 0; i < hotbar.Count; i++)
            {
                if (hotbar[i].IsNullOrEmpty())
                {
                    slot = i;
                    break;
                }
            }
        }
        if (slot == -1)
        {
            Debug.LogError("Inventory is full!");
            return;
        }
        if (!hotbar[slot].IsNullOrEmpty()) return;
        if (itemManager.itemSearch.ContainsKey(itemName))
        {
            hotbar[slot] = itemName;
            EquipItem(equippedSlot, slot == equippedSlot);
        }
        UpdateHotbarUI();
    }

    /// <summary>
    /// Removes an item from this player.
    /// </summary>
    /// <param name="itemName">Name of item</param>
    /// <param name="slot">Slot to remove item from, finds the item automatically by default</param>
    [PunRPC]
    public void RemoveItem(string itemName, int slot = -1)
    {
        if (slot == -1)
        {
            for (int i = 0; i < hotbar.Count; i++)
            {
                if (hotbar[i] == itemName)
                {
                    hotbar[i] = "";
                    EquipItem(equippedSlot, i == equippedSlot);
                    break;
                }
            }
        }
        else
        {
            if (hotbar[slot] == itemName)
            {
                EquipItem(equippedSlot, slot == equippedSlot);
            }
        }
        UpdateHotbarUI();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
