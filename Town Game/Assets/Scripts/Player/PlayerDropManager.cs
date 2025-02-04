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

    void CheckCanPlace()
    {
        Item currentItem = GetCurrentItem();
        if (currentItem == null) return;
        Vector3 direction = Quaternion.Euler(player.camDirectionX, player.camDirection, 0f) * Vector3.forward;
        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, direction, out hit, dropDistance, (int)environmentMask))
        {
            ChangeDropPosition(direction, hit.normal, hit.point, currentItem);
        } 
        else
        {
            SetMaterial(false);
            Vector3 falseDirection = Quaternion.Euler(player.camDirectionX, player.camDirection, 0f) * Vector3.forward;
            ChangeDropPosition(Quaternion.Euler(player.camDirectionX - 90f, player.camDirection, 0f) * Vector3.forward, -falseDirection, camTransform.position + falseDirection * dropDistance, currentItem);
        }
    }

    void ChangeDropPosition(Vector3 direction, Vector3 up, Vector3 point, Item currentItem)
    {
        Bounds itemBounds = currentItem.mesh.bounds;
        Vector3 forward = Vector3.ProjectOnPlane(direction, up);
        Quaternion orientation = Quaternion.LookRotation(forward, up) * Quaternion.Euler(currentItem.placedRotation); // Sets orientation on the normal
        float boundsY = GetBoundsYPosition(itemBounds.min, itemBounds.max, itemBounds.center, currentItem.placedRotation);
        Vector3 yCenter = RotatePointAroundPivot(new Vector3(0f, itemBounds.center.y, 0f), Vector3.zero, currentItem.placedRotation);
        Vector3 visualPosition = point + up * (boundsY - yCenter.y);
        float boundsX = GetBoundsYPosition(itemBounds.min, itemBounds.max, itemBounds.center, currentItem.placedRotation + new Vector3(0f, 0f, 90f));
        visualPosition -= forward.normalized * (boundsX);
        itemDrop.position = visualPosition;
        itemDrop.rotation = orientation;
    }

    /// <summary>
    /// Gets the y extent of a rotated bounding box
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    float GetBoundsYPosition(Vector3 min, Vector3 max, Vector3 center, Vector3 rotation) // what the fuc
    {
        Vector3[] points = new Vector3[8];
        points[0] = min;
        points[1] = max;
        points[2] = new Vector3(max.x, min.y, min.z);
        points[3] = new Vector3(min.x, min.y, max.z);
        points[4] = new Vector3(min.x, max.y, min.z);
        points[5] = new Vector3(min.x, max.y, max.z);
        points[6] = new Vector3(max.x, max.y, min.z);
        points[7] = new Vector3(max.x, min.y, max.z);
        float lowestPoint = Mathf.Infinity;
        foreach (Vector3 point in points)
        {
            Vector3 newPoint = RotatePointAroundPivot(point, center, rotation);
            if (newPoint.y < lowestPoint)
            {
                lowestPoint = newPoint.y;
            }
        }

        return center.y - lowestPoint;
    }

    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
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
