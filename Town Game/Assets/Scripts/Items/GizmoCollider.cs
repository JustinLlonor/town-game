using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoCollider : MonoBehaviour
{
    public LayerMask itemEnvironment;
    private List<Collider> environmentColliders = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if (Contains(itemEnvironment, other.gameObject.layer) && !environmentColliders.Contains(other))
        {
            environmentColliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (environmentColliders.Contains(other))
        {
            environmentColliders.Remove(other);
        }
    }

    private void OnDisable()
    {
        environmentColliders.Clear();
    }

    public bool ColliderTouchingEnvironment()
    {
        return environmentColliders.Count > 0;
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
