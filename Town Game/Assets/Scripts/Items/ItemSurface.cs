using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemSurface : MonoBehaviour
{
    public MapRoom property;
    public BoxCollider innerBounds;
    [Tooltip("Doesn't need to be assigned, if this is left to null then it takes from innerBounds")]
    public BoxCollider outerBounds;
    public float outerExtent = 0.025f;

    private void OnEnable()
    {
        if (outerBounds == null)
        {
            CreateOuterBounds();
        }
    }

    void CreateOuterBounds()
    {
        outerBounds = transform.GetChild(0).AddComponent<BoxCollider>();
        outerBounds.size = innerBounds.size + Vector3.one * outerExtent;
        outerBounds.center = innerBounds.center;
    }

    /// <summary>
    /// Returns true if a mesh collider is within this item surface's bounds
    /// </summary>
    /// <returns></returns>
    public bool WithinSurface(MeshCollider meshCollider, List<GameObject> collisions)
    {
        Vector3[] colliderPoints = GetBoxColliderVertices(meshCollider);
        if (collisions.Contains(gameObject))
        {
            if (!PointsWithinCollider(colliderPoints, outerBounds)) // If all the points are not within the outer bounds, return false
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Returns false if there is at least one point outside of the collider
    /// </summary>
    /// <param name="points"></param>
    /// <param name="collider"></param>
    /// <returns></returns>
    public bool PointsWithinCollider(Vector3[] points, Collider collider) // Can be adjusted later to compensate for different types of colliders
    {
        foreach (Vector3 point in points)
        {
            if (!collider.bounds.Contains(point)) // If the collider doesn't contain the point return false
            {
                return false;
            }
        }
        return true; // Returns true if there are no points which the collider does not encapsulate
    }

    // Credit: Bunny83 on the Unity forums
    public Vector3[] GetBoxColliderVertices(MeshCollider col) // Gets the  collider vertices
    {
        var trans = col.transform;
        var min = col.bounds.center - col.bounds.size * 0.5f;
        var max = col.bounds.center + col.bounds.size * 0.5f;

        var P000 = trans.TransformPoint(new Vector3(min.x, min.y, min.z));
        var P001 = trans.TransformPoint(new Vector3(min.x, min.y, max.z));
        var P010 = trans.TransformPoint(new Vector3(min.x, max.y, min.z));
        var P011 = trans.TransformPoint(new Vector3(min.x, max.y, max.z));
        var P100 = trans.TransformPoint(new Vector3(max.x, min.y, min.z));
        var P101 = trans.TransformPoint(new Vector3(max.x, min.y, max.z));
        var P110 = trans.TransformPoint(new Vector3(max.x, max.y, min.z));
        var P111 = trans.TransformPoint(new Vector3(max.x, max.y, max.z));

        Vector3[] output = new Vector3[] { P000, P001, P010, P011, P100, P101, P110, P111};
        return output;
    }
}
