using Fusion;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoManager : MonoBehaviour
{
    /// <summary>
    /// The player this gizmo is tracking
    /// </summary>
    public Player attachedPlayer;
    public float rotation;
    public float errorDisplacement = 0.05f;
    public float surfaceTolerance = 0.95f;
    public LayerMask environmentMask;
    public Material[] gizmoMaterials;
    public Material errorMaterial;
    //public Item testItem;
    [Header("References")]
    // Graphics parent transform
    public Transform gizmoRotationPivot;
    public Transform graphicsTransform;
    public Transform GFXRotationPivot;
    public GizmoCollider gizmoCollider;
    public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    [HideInInspector] public Transform camTransform;
    [HideInInspector] public bool gizmoEnabled = false;
    private GizmoSettings currentSettings;
    private DeviceVolume currentDeviceVolume;
    private RunnerManager rm;
    private PositionManager positionManager;
    private bool graphicsShown = false;
    private bool unparentedGFX = false;
    private bool onSurface = false;
    private Vector3 surfaceNormal = Vector3.zero;

    private struct GizmoPlacementInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool gizmoOnSurface;
        public Vector3 surfaceNormal;

        public GizmoPlacementInfo(Vector3 position, Quaternion rotation, bool gizmoOnSurface, Vector3 surfaceNormal)
        {
            this.position = position;
            this.rotation = rotation;
            this.gizmoOnSurface = gizmoOnSurface;
            this.surfaceNormal = surfaceNormal;
        }
    }

    public struct PlacementInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public GizmoMode mode;
        public ItemSurface itemSurface;

        public PlacementInfo(Vector3 position, Quaternion rotation, GizmoMode mode, ItemSurface itemSurface)
        {
            this.position = position;
            this.rotation = rotation;
            this.mode = mode;
            this.itemSurface = itemSurface;
        }
    }

    private void Awake()
    {
        rm = FindAnyObjectByType<RunnerManager>();
        positionManager = FindAnyObjectByType<PositionManager>();
    }

    private void Update()
    {
        if (graphicsShown && gizmoEnabled)
        {
            float localCamX = rm.camOrientation;
            float localCamY = rm.orientation;
            PlaceGraphics(Quaternion.Euler(localCamX, localCamY, 0f) * Vector3.forward);
        }
    }

    public void EnterLookMode(Mesh gizmoMesh, GizmoSettings settings, bool showGraphics)
    {
        if (gizmoEnabled) return;
        gizmoCollider.ResetColliders();
        CreateGizmo(settings, gizmoMesh);
        if (settings.gizmoMode == GizmoMode.Device)
        {
            if (attachedPlayer.connectedPanel != null)
            {
                Debug.Log("setting");
                currentDeviceVolume = attachedPlayer.connectedPanel.connectedVolume;
            }
            else Debug.Log("no connected panel");
        }
        graphicsShown = showGraphics;
        if (showGraphics)
        {
            meshRenderer.enabled = true;
            meshFilter.mesh = gizmoMesh;
            SetModeMat();
        }
        gizmoEnabled = true;
    }

    public void ExitLookMode()
    {
        gizmoEnabled = false;
        DisableGizmo();
    }

    public void LookModeRaycast()
    {
        float camX;
        float camY;
        camX = attachedPlayer.camDirectionX;
        camY = attachedPlayer.camDirection;
        Vector3 direction = Quaternion.Euler(camX, camY, 0f) * Vector3.forward;
        GizmoPlacementInfo placementInfo = GetPlacementInfo(direction, true);
        transform.position = placementInfo.position;
        transform.rotation = placementInfo.rotation;
        onSurface = placementInfo.gizmoOnSurface;
        surfaceNormal = placementInfo.surfaceNormal;
        UpdateIndicator();
    }

    private void PlaceGraphics(Vector3 direction)
    {
        if (!unparentedGFX)
        {
            graphicsTransform.parent = null;
            unparentedGFX = true;
        }
        GizmoPlacementInfo placementInfo = GetPlacementInfo(direction);
        graphicsTransform.position = placementInfo.position;
        graphicsTransform.rotation = placementInfo.rotation;
    }

    /// <summary>
    /// Gets the placement info given the direction from the player
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private GizmoPlacementInfo GetPlacementInfo(Vector3 direction, bool useErrorDisplacement = false)
    {
        GizmoPlacementInfo output = new GizmoPlacementInfo();
        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, direction, out hit, currentSettings.placementRange, (int)environmentMask))
        {
            // Error displacement is for checks
            if (useErrorDisplacement) output.position = hit.point + hit.normal * errorDisplacement;
            else output.position = hit.point;
            // If the item is on a floor or ceiling surface, point to the player, otherwise, point up
            // TODO: Make the ceiling and floor dots actually have good up vectors?
            output.surfaceNormal = hit.normal;
            if (Vector3.Dot(hit.normal, Vector3.up) >= surfaceTolerance)
            {
                Vector3 forward = output.position - camTransform.position;
                forward = new Vector3(forward.x, 0f, forward.z).normalized;
                if (forward.Equals(Vector3.zero)) forward = Quaternion.Euler(0f, attachedPlayer.camDirection, 0f) * Vector3.forward;
                Quaternion outputRotation = Quaternion.LookRotation(forward, Vector3.up);
                Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.green);
                Debug.DrawLine(transform.position, transform.position + forward, Color.red);
                output.rotation = outputRotation;
            }
            else if (Vector3.Dot(hit.normal, Vector3.down) >= surfaceTolerance)
            {
                Vector3 forward = camTransform.position - output.position;
                forward = new Vector3(forward.x, 0f, forward.z).normalized;
                if (forward.Equals(Vector3.zero)) forward = Quaternion.Euler(0f, attachedPlayer.camDirection, 0f) * Vector3.back;
                Quaternion outputRotation = Quaternion.LookRotation(forward, Vector3.down);
                Debug.DrawLine(transform.position, transform.position + Vector3.down, Color.green);
                Debug.DrawLine(transform.position, transform.position + forward, Color.red);
                output.rotation = outputRotation;
            }
            else
            {
                Vector3 right = Vector3.Cross(Vector3.up, hit.normal);
                Vector3 up = Vector3.Cross(hit.normal, right);
                Quaternion outputRotation = Quaternion.LookRotation(up, hit.normal);
                output.rotation = outputRotation;
                Debug.DrawLine(transform.position, transform.position + up, Color.green);
                Debug.DrawLine(transform.position, transform.position + hit.normal, Color.red);
            }
            output.gizmoOnSurface = true;
            return output;
        }
        output.position = camTransform.position + (direction.normalized * currentSettings.placementRange);
        Vector3 fDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        if (fDirection.Equals(Vector3.zero)) fDirection = Quaternion.Euler(0f, attachedPlayer.camDirection, 0f) * Vector3.forward;
        output.rotation = Quaternion.LookRotation(fDirection, Vector3.up);
        output.surfaceNormal = Vector3.zero;
        output.gizmoOnSurface = false;
        return output;
    }

    private void CreateGizmo(GizmoSettings settings, Mesh mesh)
    {
        meshCollider.enabled = true;
        onSurface = false;
        rotation = settings.rotationSettings.initialRotation;
        Vector3 initRotVector = new Vector3(0f, settings.rotationSettings.initialRotation, 0f);
        currentSettings = settings;
        meshCollider.sharedMesh = mesh;
        gizmoRotationPivot.localEulerAngles = initRotVector;
        GFXRotationPivot.localEulerAngles = initRotVector;
        meshCollider.transform.localEulerAngles = settings.rotation;
        meshRenderer.transform.localEulerAngles = settings.rotation;
        ReadCenterSettings(settings, mesh);
        ReadUpSettings(settings, mesh);
    }

    public void DisableGizmo()
    {
        meshCollider.enabled = false;
        meshRenderer.enabled = false;
    }
    
    //TODO: Make device system use itemsurface component as well
    /// <summary>
    /// Determines if the placement of the gizmo is valid
    /// </summary>
    /// <returns></returns>
    public bool PlacementValid()
    {
        // if its touching any surface, return false
        if (gizmoCollider.ColliderTouchingEnvironment()) return false;
        // if not attached to a surface, then return false
        if (!onSurface) return false;
        // Surface normal angle check
        float normalAngle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(surfaceNormal, Vector3.up), -1f, 1f)) * Mathf.Rad2Deg;
        if (!currentSettings.rotationSettings.surfaceRotationLimit.RotationWithinLimit(normalAngle)) return false;
        // Item surface check
        if (currentSettings.gizmoMode == GizmoMode.Item)
        {
            ItemSurface foundSurface = gizmoCollider.GetItemSurface();
            if (foundSurface != null)
            {
                if (!positionManager.PlayerHasAccessToRoom(attachedPlayer.owner, foundSurface.property.roomName)) return false;
            }
            else return false;
        }
        else if (currentSettings.gizmoMode == GizmoMode.Device) // Device volume check
        {
            if (currentDeviceVolume == null) return false;
            if (!gizmoCollider.ColliderInDeviceVolume(currentDeviceVolume.volumeCollider)) return false;
        }
        return true;
    }

    /// <summary>
    /// Displaces the collider up so that it touches the ground
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="mesh"></param>
    private void ReadUpSettings(GizmoSettings settings, Mesh mesh)
    {
        if (!settings.upSettings.useMeshData)
        {
            meshCollider.transform.localPosition = new Vector3(meshCollider.transform.localPosition.x, 
                settings.upSettings.upDisplacement, meshCollider.transform.localPosition.z);
            return;
        }
        GizmoAxis upAxis = settings.GetUpAxis();
        float upExtent = GetMeshExtent(mesh, upAxis);
        float upCenter = GetMeshCenter(mesh, upAxis);
        float newY = settings.upSettings.upDisplacement + errorDisplacement;
        // if the axis is negative, then flip it and use the negative up center
        if (upAxis.IsNegative()) upCenter *= -1;
        newY += -upCenter + upExtent;
        meshCollider.transform.localPosition = new Vector3(meshCollider.transform.localPosition.x,
                newY, meshCollider.transform.localPosition.z);
        meshRenderer.transform.localPosition = new Vector3(meshRenderer.transform.localPosition.x,
                newY, meshRenderer.transform.localPosition.z);
    }

    private void ReadCenterSettings(GizmoSettings settings, Mesh mesh)
    {
        CenterAxes(settings.GetXAxis(), settings.GetZAxis(), settings.centerSettings.centerX, settings.centerSettings.centerZ, settings.centerSettings.displacement, mesh);
    }

    public void Rotate(float rotDelta)
    {
        float previousRotation = rotation;
        float newRotation = rotation + rotDelta;
        rotation = currentSettings.rotationSettings.rotationLimit.ClampAngle(newRotation, previousRotation);
        gizmoRotationPivot.localEulerAngles = new Vector3(0f, rotation, 0f);
        GFXRotationPivot.localEulerAngles = new Vector3(0f, rotation, 0f);
    }

    private void CenterAxes(GizmoAxis meshXAxis, GizmoAxis meshZAxis, bool centerX, bool centerZ, Vector2 displacement, Mesh mesh)
    {
        // Gets the centers from the mesh depending on the mesh's x and z axis
        float meshDisplacementX = GetMeshCenter(mesh, meshXAxis);
        float meshDisplacementZ = GetMeshCenter(mesh, meshZAxis);
        // Sets the new x and z, depending on if we want to center the axes
        float newX = -meshDisplacementX;
        if (meshXAxis.IsNegative()) newX *= -1;
        float newZ = -meshDisplacementZ;
        if (meshZAxis.IsNegative()) newZ *= -1;
        if (!centerX) newX = displacement.x;
        if (!centerZ) newZ = displacement.y;
        // Sets local pos
        meshCollider.transform.localPosition = new Vector3(newX, meshCollider.transform.localPosition.y, newZ);
        meshRenderer.transform.localPosition = new Vector3(newX, meshRenderer.transform.localPosition.y, newZ);
    }

    private float GetMeshExtent(Mesh mesh, GizmoAxis axis)
    {
        if (axis == GizmoAxis.PosX) return mesh.bounds.extents.x;
        if (axis == GizmoAxis.PosY) return mesh.bounds.extents.y;
        if (axis == GizmoAxis.PosZ) return mesh.bounds.extents.z;
        if (axis == GizmoAxis.NegX) return mesh.bounds.extents.x;
        if (axis == GizmoAxis.NegY) return mesh.bounds.extents.y;
        return mesh.bounds.extents.z;
    }

    private float GetMeshCenter(Mesh mesh, GizmoAxis axis)
    {
        if (axis == GizmoAxis.PosX) return mesh.bounds.center.x;
        if (axis == GizmoAxis.PosY) return mesh.bounds.center.y;
        if (axis == GizmoAxis.PosZ) return mesh.bounds.center.z;
        if (axis == GizmoAxis.NegX) return mesh.bounds.center.x;
        if (axis == GizmoAxis.NegY) return mesh.bounds.center.y;
        return mesh.bounds.center.z;
    }

    public void HideGizmo()
    {
        meshCollider.enabled = false;
        meshRenderer.enabled = false;
    }

    private void SetModeMat()
    {
        meshRenderer.material = gizmoMaterials[(int)currentSettings.gizmoMode];
    }

    private void SetErrorMat()
    {
        meshRenderer.material = errorMaterial;
    }

    /// <summary>
    /// Updates the material for the gizmo based on if the placement is valid or not
    /// </summary>
    private void UpdateIndicator()
    {
        bool placementValid = PlacementValid();
        if (placementValid)
        {
            SetModeMat();
            return;
        }
        SetErrorMat();
    }

    public PlacementInfo GetPlacementInfo()
    {
        Vector3 newPos = meshCollider.transform.position - surfaceNormal.normalized * errorDisplacement;
        return new PlacementInfo(newPos, meshCollider.transform.rotation, currentSettings.gizmoMode, gizmoCollider.GetItemSurface());
    }
}
