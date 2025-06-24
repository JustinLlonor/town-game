using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using WebSocketSharp;

public class PlayerDropManager : NetworkBehaviour
{
    [HideInInspector] public PlayerInventory inventory;
    public bool dropPressed = false;
    [HideInInspector] public Player player;
    bool previousPressed = false;
    [Header("Drop Settings")]
    public float dropDistance = 4f;
    public LayerMask environmentMask;
    public float surfaceTolerance = 0.95f;
    [Header("References")]
    public GameObject gizmoPrefab;
    public NetworkPrefabRef physItem;
    public NetworkPrefabRef itemGizmo;
    public Transform camTransform;
    public ItemGizmo gizmo;
    [Networked] public bool isPlacing { get; set; } = false;
    [Networked] public bool isRotating { get; set; } = false;
    private bool previousRotating = false;
    bool isPlace = false;
    bool rotationPressed = false;
    GizmoManager gizmoManager;
    RunnerManager rm;
    PositionManager positionManager;
    CameraMovement cameraMovement;
    private bool xLocked = false;

    public override void Spawned()
    {
        //if (Runner.IsServer) Runner.Spawn(itemGizmo, Vector3.zero, Quaternion.identity, Object.InputAuthority);
        if (!Object.IsProxy)
        {
            // Initialize the gizmo
            GameObject gizmoObject = Instantiate(gizmoPrefab);
            gizmoManager = gizmoObject.GetComponent<GizmoManager>();
            gizmoManager.camTransform = camTransform;
            gizmoManager.attachedPlayer = player;
        }

        rm = FindFirstObjectByType<RunnerManager>();
        positionManager = FindAnyObjectByType<PositionManager>();
        inventory.OnSwitchSlot += CancelDrop;
        if (!HasInputAuthority) return;
        InputManager inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onDropItem += OnDropItem;
        inputManager.onRotateMode += OnRotateModePressed;
        cameraMovement = FindAnyObjectByType<CameraMovement>();
        if (positionManager != null)
        {
            positionManager.onJobAdd += UpdateGizmo;
            positionManager.onJobRemove += UpdateGizmo;
        }
    }

    /// <summary>
    /// Destroy the associated gizmo manager when this player object is destroyed
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="hasState"></param>
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (!Object.IsProxy && gizmoManager != null)
        {
            Destroy(gizmoManager.gameObject);
        } 
    }

    private void Update()
    {
        /**
        if (isPlacing && HasInputAuthority)
        {
            UpdateGizmo();
        }
        **/
    }

    public override void FixedUpdateNetwork()
    {
        if (gizmoManager == null) return;
        if (Runner.IsResimulation) return;
        if (Object.HasInputAuthority && xLocked && !isPlacing)
        {
            cameraMovement.UnlockX();
            xLocked = false;
        }
        // Change detector
        if (previousPressed != dropPressed)
        {
            previousPressed = dropPressed;
            if (dropPressed)
            {
                isRotating = false;
                Debug.Log("drop pressed 2");
                OnDropPressed();
            }
            if (!dropPressed)
            {
                Debug.Log("drop released");
                OnDropRelease();
            }
        }
        if (!Object.IsProxy && isPlacing)
        {
            gizmoManager.LookModeRaycast();
            //UpdateGizmo();
        }
        if (!isPlacing) return;
        if (previousRotating != isRotating)
        {
            previousRotating = isRotating;
            if (!HasInputAuthority) return;
            if (isRotating)
            {
                cameraMovement.LockX();
                xLocked = true;
            }
            if (!isRotating)
            {
                cameraMovement.UnlockX();
                xLocked = false;
                rm.lockedDelta = 0f;
            }
        }
        if (isRotating && HasInputAuthority)
        {
            float newDelta = cameraMovement.GetLockedDelta();
            rm.lockedDelta = newDelta;
        }
    }

    // When drop pressed down
    void OnDropPressed()
    {
        Item currentItem = GetCurrentItem();
        // if we are not holding an item, return
        Debug.Log("drop pressed function call");
        if (currentItem == null) return;
        // Enable mesh renderer
        gizmoManager.EnterLookMode(currentItem.mesh, currentItem.dropSettings, Object.HasInputAuthority);
        //if (HasInputAuthority) gizmo.SetRenderer(true, currentItem.mesh);
        //gizmo.SetColliderBounds(currentItem.mesh.bounds.center, currentItem.mesh.bounds.size);
        isPlacing = true;
    }

    void OnDropRelease()
    {
        VerifyPlacement();
        CancelDrop();
    }

    void CancelDrop()
    {
        //gizmo.SetRenderer(false);
        gizmoManager.ExitLookMode();
        isPlacing = false;
        if (isRotating)
        {
            if (Object.HasInputAuthority)
            {
                cameraMovement.UnlockX();
                xLocked = true;
            }
            isRotating = false;
            previousRotating = false;
        }
        return;
    }

    void VerifyPlacement() // Places the item if the item placement is valid
    {
        if (!gizmoManager.PlacementValid()) return;
        GizmoManager.PlacementInfo placeInfo = gizmoManager.GetPlacementInfo();
        if (placeInfo.mode == GizmoMode.Item)
        {
            PlaceItem(placeInfo.position, placeInfo.rotation, placeInfo.itemSurface);
        }
        else if (placeInfo.mode == GizmoMode.Device)
        {

        }
        // Play an error sfx
    }

    void PlaceItem(Vector3 position, Quaternion rotation, ItemSurface iSurface)
    {
        Item item = GetCurrentItem();
        if (item == null) return;
        if (inventory.hotbar[inventory.equippedSlot].ToString().IsNullOrEmpty()) return;
        if (!Runner.IsServer && HasInputAuthority) inventory.RemoveItem(inventory.equippedSlot);
        if (!Runner.IsServer) return;
        NetworkObject itemObj = Runner.Spawn(physItem, position, rotation);
        ItemPhys pItem = itemObj.GetComponent<ItemPhys>();
        pItem.itemName = item.name;
        pItem.gameObject.name = item.name;
        pItem.room = iSurface.property.roomName;
        pItem.rolesRevealed = true;
        TransferItemData(inventory.itemData[inventory.equippedSlot], pItem);

        inventory.RemoveItem(inventory.equippedSlot);
        //RemoveItem(hotbar[equippedSlot].ToString(), equippedSlot);
    }

    void TransferItemData(ItemData data, ItemPhys physItem)
    {
        physItem.itemData = new ItemData(data.metadata, data.fingerprints);
    }

    public void ReceiveRotationDelta(float rotDelta)
    {
        gizmoManager.Rotate(rotDelta);
    }

    private void UpdateGizmo(Vector2Int jobRef)
    {
        //TODO: Create a function that checks if the placement is valid

        //UpdateGizmo();
    }

    void UpdateGizmo()
    {
        Item currentItem = GetCurrentItem();
        if (currentItem == null) return;
        float camX;
        float camY;
        if (!Runner.IsServer)
        {
            camX = rm.camOrientation;
            camY = rm.orientation;
        } else
        {
            camX = player.camDirectionX;
            camY = player.camDirection;
        }
        Vector3 direction = Quaternion.Euler(camX, camY, 0f) * Vector3.forward;
        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, direction, out hit, dropDistance, (int)environmentMask))
        {
            ChangeDropPosition(direction, hit.normal, hit.point, currentItem);
            if (Vector3.Dot(hit.normal, Vector3.up) < surfaceTolerance) // If the dot product is not close enough to Vector3.up invalidate the placement
            {
                isPlace = false;
                gizmo.SetMaterial(false);
                return;
            }
            isPlace = true;
            gizmo.checkForCollisions = true;
            ItemSurface iSurface = gizmo.GetItemSurface();
            gizmo.SetMaterial(iSurface != null && positionManager.PlayerHasAccessToRoom(Runner.LocalPlayer, iSurface.property.roomName));
        } 
        else
        {
            isPlace = false;
            gizmo.checkForCollisions = false;
            gizmo.SetMaterial(false);
            Vector3 falseDirection = Quaternion.Euler(camX, camY, 0f) * Vector3.forward;
            ChangeDropPosition(Quaternion.Euler(camX - 90f, camY, 0f) * Vector3.forward, -falseDirection, camTransform.position + falseDirection * dropDistance, currentItem);
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
        gizmo.ChangePosition(visualPosition, orientation);
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
        Debug.Log("drop pressed");
        if (iv.Get<float>() == 0f)
        {
            rm.dropPressed = false;
            return;
        }
        rm.dropPressed = true;
    }

    private void OnRotateModePressed(InputValue iv)
    {
        if (iv.Get<float>() == 0f)
        {
            rm.rotateModePressed = false;
            return;
        }
        rm.rotateModePressed = true;
    }
}
