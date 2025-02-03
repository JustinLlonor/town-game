using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class PlayerDropManager : NetworkBehaviour
{
    [HideInInspector] public PlayerInventory inventory;
    public bool dropPressed = false;
    [HideInInspector] public Player player;
    bool previousPressed = false;
    [Header("Drop Settings")]
    public float dropDistance = 4f;
    public float surfaceTolerance = 0.923879533f;
    public LayerMask environmentMask;
    public float groundDistance = 0.01f;
    [Header("References")]
    public NetworkPrefabRef physItem;
    public Transform itemDrop;
    public Transform camTransform;
    public Material canPlace;
    public Material cantPlace;
    bool isPlacing = false;
    RunnerManager rm;
    MeshRenderer itemRenderer;
    MeshFilter itemFilter;

    public override void Spawned()
    {
        rm = FindObjectOfType<RunnerManager>();
        itemRenderer = itemDrop.GetComponent<MeshRenderer>();
        itemFilter = itemDrop.GetComponent<MeshFilter>();
    }

    public override void FixedUpdateNetwork()
    {
        // Change detector
        if (previousPressed != dropPressed)
        {
            previousPressed = dropPressed;
            if (dropPressed)
            {
                OnDropPressed();
            }
            if (!dropPressed)
            {
                OnDropRelease();
            }
        }
        if (isPlacing)
        {
            CheckCanPlace();
        }
    }

    // When drop pressed down
    void OnDropPressed()
    {
        Item currentItem = GetCurrentItem();
        // if we are not holding an item, return
        if (currentItem == null) return;
        // Enable mesh renderer
        itemRenderer.enabled = true;
        itemFilter.mesh = currentItem.mesh;
        isPlacing = true;
        itemDrop.GetComponent<BoxCollider>().center = currentItem.mesh.bounds.center;
        itemDrop.GetComponent<BoxCollider>().size = currentItem.mesh.bounds.size;
    }

    void OnDropRelease()
    {
        itemRenderer.enabled = false;
        isPlacing = false;
    }

    void SetMaterial(bool allowedPlace)
    {
        if (allowedPlace)
        {
            itemRenderer.material = canPlace;
            return;
        }
        itemRenderer.material = cantPlace;
    }

    void CheckCanPlace() // Jesus Christ
    {
        Item currentItem = GetCurrentItem();
        if (currentItem == null) return;
        Bounds itemBounds = currentItem.mesh.bounds;
        Vector3 direction = Quaternion.Euler(player.camDirectionX, player.camDirection, 0f) * Vector3.forward;
        Quaternion orientation = Quaternion.Euler(0f, -90f + player.camDirection, -90f);
        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, direction, out hit, dropDistance, (int)environmentMask))
        {
            Vector3 normalDirection = hit.normal;
            Vector3 newPoint = hit.point + Vector3.up * (itemBounds.extents.x) * (1 + groundDistance);
            Vector3 visualPosition = new Vector3(direction.x, 0f, direction.z).normalized;
            visualPosition = newPoint - visualPosition * itemBounds.extents.y;
            //newRotation *= orientation;
            itemDrop.position = visualPosition;
            itemDrop.rotation = orientation;
            if (!Physics.CheckBox(newPoint, itemBounds.size/2f, orientation, (int)environmentMask))
            {
                if (Vector3.Dot(normalDirection, Vector3.up) >= surfaceTolerance)
                {
                    SetMaterial(true);
                    return;
                }
            } else
            {
                SetMaterial(false);
            }
        } else
        {
            SetMaterial(false);
            Vector3 newPoint = hit.point + Vector3.up * (itemBounds.extents.x) * (1 + groundDistance);
            Vector3 visualPosition = new Vector3(direction.x, 0f, direction.z).normalized;
            visualPosition = newPoint - visualPosition * itemBounds.extents.y; // what the fuc is going on 
            itemDrop.position = visualPosition;
            itemDrop.rotation = orientation;
        }
    }

    Item GetCurrentItem()
    {
        return inventory.equippedItem;
    }

    private void OnDropItem(InputValue iv)
    {
        if (iv.Get<float>() == 0f)
        {
            rm.dropPressed = false;
            return;
        }
        rm.dropPressed = true;
    }
}
