using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    [Header("View info")]
    public Vector3 viewPosition;
    public float viewRotation;
    public float zoomRadius;
    public float viewElevation;
    public MapRoom viewRoom;
    public string trackedIcon;
    [Header("Interaction settings")]
    public bool isPannable;
    public bool isZoomable;
    public float minZoomRadius;
    public float maxZoomRadius;

    private float lerpZoomRadius;
    private float lerpPosition;

    public void SetPosition(Vector3 position)
    {
        viewPosition = Vector3.zero;
    }

    public void SetLerpPosition(Vector3 position)
    {

    }

    public void SetRotation(float rotation)
    {

    }

    public void SetZoom(float zoomRadius)
    {

    }

    public void SetLerpZoom(float zoomRadius)
    {

    }

    /// <summary>
    /// Note (to be deleted later) pointers that point to a higher elevation will oscillate between normal size and larger for an animation,
    /// pointers that point to a lower elevation will oscillate between normal and smaller for an animation. Both happen somewhat slowly and subtly
    /// </summary>
    /// <param name="pointLocation"></param>
    /// <param name="color"></param>
    public void AddPointer(string name, Vector3 pointLocation, Color color)
    {

    }

    public void RemovePointer(string name)
    {

    }
}
