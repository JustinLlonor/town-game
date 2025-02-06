using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ItemGizmo : NetworkBehaviour
{
    public List<GameObject> currentCollisions = new List<GameObject>();
    public LayerMask itemMask;
    [HideInInspector] public MeshRenderer mRenderer;
    [HideInInspector] public MeshFilter filter;
    [HideInInspector] public Material canPlace;
    [HideInInspector] public Material cantPlace;
    [HideInInspector] public BoxCollider bCollider;
    [HideInInspector] public bool checkForCollisions = false;

    public override void Spawned()
    {
        StartCoroutine(AssignCoroutine());
    }

    public override void FixedUpdateNetwork()
    {
        UpdateColliders();
    }

    void UpdateColliders() // Updates the collision list
    {
        if (!checkForCollisions) return;
        Collider[] colliders = Physics.OverlapBox(transform.position, bCollider.size / 2f, transform.rotation, (int)itemMask);
        currentCollisions.Clear();
        foreach (Collider collider in colliders)
        {
            currentCollisions.Add(collider.gameObject);
        }
    }

    IEnumerator AssignCoroutine()
    {
        PlayerManager playerManager = FindFirstObjectByType<PlayerManager>();
        while (!playerManager.playerObjects.ContainsKey(Object.InputAuthority)) // While the player manager doesn't contain the key of input authority we wait
        {
            yield return null;
        }
        playerManager.playerObjects[Object.InputAuthority].GetComponent<PlayerDropManager>().gizmo = this;
    }

    public bool CheckPlaceable()
    {
        foreach (GameObject collision  in currentCollisions)
        {
            ItemSurface iSurface = collision.GetComponent<ItemSurface>();
            if (iSurface != null)
            {
                if (iSurface.WithinSurface(bCollider, currentCollisions)) // If this item gizmo is within the box collider's surface, return true
                {
                    return true;
                }
            }
        }
        return false; // Returns false if the item is within no item surface
    }

    public void SetMaterial(bool allowedPlace)
    {
        if (allowedPlace)
        {
            mRenderer.material = canPlace;
            return;
        }
        mRenderer.material = cantPlace;
    }

    public void ChangePosition(Vector3 position, Quaternion orientation)
    {
        transform.position = position;
        transform.rotation = orientation;
    }

    public void SetRenderer(bool enabled = true, Mesh mesh = null)
    {
        mRenderer.enabled = enabled;
        if (!enabled) return;
        filter.mesh = mesh;
    }

    public void SetColliderBounds(Vector3 center, Vector3 size)
    {
        bCollider.center = center;
        bCollider.size = size;
    }
}
