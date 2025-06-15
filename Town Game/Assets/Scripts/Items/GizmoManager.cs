using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GizmoManager : MonoBehaviour
{
    public float errorDisplacement = 0.05f;
    public Item testItem;
    [Header("References")]
    public GizmoCollider gizmoCollider;
    public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    private bool gizmoEnabled = false;
    private GizmoSettings currentSettings;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            CreateGizmo(testItem.dropSettings, testItem.mesh);
        }
    }

    public void CreateGizmo(GizmoSettings settings, Mesh mesh)
    {
        currentSettings = settings;
        gizmoEnabled = true;
        meshCollider.enabled = true;
        meshRenderer.enabled = true;
        meshFilter.mesh = mesh;
        meshCollider.transform.localPosition = Vector3.zero;
        meshCollider.transform.localEulerAngles = settings.rotation;
        ReadCenterSettings(settings, mesh);
        ReadUpSettings(settings, mesh);
    }

    public void DisableGizmo()
    {
        gizmoEnabled = false;
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
