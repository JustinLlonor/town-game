using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoCollider : MonoBehaviour
{
    public MeshCollider meshCollider;
    public LayerMask itemEnvironment;
    public LayerMask deviceVolume;
    public List<GameObject> currentColliders = new List<GameObject>();
    private List<Collider> environmentColliders = new List<Collider>();
    private List<Collider> deviceVolumeColliders = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");
        currentColliders.Add(other.gameObject);
        if (Contains(itemEnvironment, other.gameObject.layer) && !environmentColliders.Contains(other))
        {
            environmentColliders.Add(other);
        }
        if (Contains(deviceVolume, other.gameObject.layer) && !deviceVolumeColliders.Contains(other))
        {
            deviceVolumeColliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exited");
        if (currentColliders.Contains(other.gameObject)) currentColliders.Remove(other.gameObject);
        if (environmentColliders.Contains(other))
        {
            environmentColliders.Remove(other);
        }
        if (deviceVolumeColliders.Contains(other))
        {
            deviceVolumeColliders.Remove(other);
        }
    }

    public void ResetColliders()
    {
        currentColliders.Clear();
        environmentColliders.Clear();
        deviceVolumeColliders.Clear();
    }

    public ItemSurface GetItemSurface()
    {
        foreach (GameObject collision in currentColliders)
        {
            ItemSurface iSurface = collision.GetComponent<ItemSurface>();
            if (iSurface != null)
            {
                if (iSurface.WithinSurface(meshCollider, currentColliders)) // If this item gizmo is within the box collider's surface, return true
                {
                    Debug.Log("Checking within surface");
                    return iSurface;
                }
            }
        }
        return null; // Returns false if the item is within no item surface
    }

    public bool ColliderTouchingEnvironment()
    {
        return environmentColliders.Count > 0;
    }

    public bool ColliderInDeviceVolume(Collider deviceVolumeCollider)
    {
        return deviceVolumeColliders.Contains(deviceVolumeCollider);
    }

    /// <summary>
    /// If a layer mask contains a value
    /// </summary>
    /// <param name="val"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    private bool Contains(LayerMask val, int layer)
    {
        return ((val & (1 << layer)) > 0);
    }
}
