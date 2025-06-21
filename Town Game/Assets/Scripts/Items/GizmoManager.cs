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
    public float errorDisplacement = 0.05f;
    public float surfaceTolerance = 0.95f;
    public LayerMask environmentMask;
    //public Item testItem;
    [Header("References")]
    public GizmoCollider gizmoCollider;
    public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    [HideInInspector] public Transform camTransform;
    [HideInInspector] public bool gizmoEnabled = false;
    private GizmoSettings currentSettings;
    private RunnerManager rm;
    private bool graphicsShown = false;
    private bool unparentedGFX = false;

    private struct GizmoPlacementInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool gizmoOnSurface;

        public GizmoPlacementInfo(Vector3 position, Quaternion rotation, bool gizmoOnSurface)
        {
            this.position = position;
            this.rotation = rotation;
            this.gizmoOnSurface = gizmoOnSurface;
        }
    }

    private void Awake()
    {
        rm = FindAnyObjectByType<RunnerManager>();
    }

    private void Update()
    {
        return;
        if (graphicsShown && gizmoEnabled)
        {
            float localCamX = rm.camOrientation;
            float localCamY = rm.orientation;
            PlaceGraphics(Quaternion.Euler(localCamX, localCamY, 0f) * Vector3.forward);
        }
    }

    public void EnterLookMode(Mesh gizmoMesh, GizmoSettings settings, bool showGraphics)
    {
        Debug.Log("look mode entered");
        graphicsShown = showGraphics;
        if (showGraphics)
        {
            meshCollider.enabled = true;
            meshRenderer.enabled = true;
            meshFilter.mesh = gizmoMesh;
        }
        CreateGizmo(settings, gizmoMesh);
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
    }

    private void PlaceGraphics(Vector3 direction)
    {
        if (!unparentedGFX)
        {
            meshRenderer.transform.parent = null;
            unparentedGFX = true;
        }
        GizmoPlacementInfo placementInfo = GetPlacementInfo(direction);
        meshRenderer.transform.position = placementInfo.position;
        meshRenderer.transform.rotation = placementInfo.rotation;
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
        output.rotation = Quaternion.LookRotation(direction);
        output.gizmoOnSurface = false;
        return output;
    }

    private void CreateGizmo(GizmoSettings settings, Mesh mesh)
    {
        currentSettings = settings;
        meshCollider.sharedMesh = mesh;
        meshCollider.transform.localPosition = Vector3.zero;
        meshCollider.transform.localEulerAngles = settings.rotation;
        ReadCenterSettings(settings, mesh);
        ReadUpSettings(settings, mesh);
    }

    public void DisableGizmo()
    {
        meshCollider.enabled = false;
        meshRenderer.enabled = false;
    }
    
    /// <summary>
    /// Determines if the placement of the gizmo is valid
    /// </summary>
    /// <returns></returns>
    public bool PlacementValid()
    {
        return !gizmoCollider.ColliderTouchingEnvironment();
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
    }

    private void ReadCenterSettings(GizmoSettings settings, Mesh mesh)
    {
        CenterAxes(settings.GetXAxis(), settings.GetZAxis(), settings.centerSettings.centerX, settings.centerSettings.centerZ, settings.centerSettings.displacement, mesh);
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
}
