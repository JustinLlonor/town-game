using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    [Header("View info")]
    public Vector3 viewPosition;
    public float viewRotation = 0f;
    public float zoomRadius = 5f;
    public MapRoom viewRoom;
    public string trackedIcon;
    [Header("Interaction settings")]
    public bool isPannable;
    public bool isZoomable;
    public float minZoomRadius;
    public float maxZoomRadius;
    [Header("References")]
    public Transform elementHolder;
    public Transform rotationPivot;

    private float lerpZoomRadius;
    private Vector3 lerpPosition;
    /// <summary>
    /// The scale of the minimap such that the x axis fits the minimap holder
    /// </summary>
    private float minimapMax;
    /// <summary>
    /// The amount of world space units the x axis is long
    /// </summary>
    private float minimapWorldSize;
    /// <summary>
    /// The amount of distance required for the element holder to go halfway on the x axis
    /// </summary>
    private float maxDistance;
    Vector3 canvasLocation;

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        MinimapManager minimapManager = FindAnyObjectByType<MinimapManager>();
        float canvasX = minimapManager.GetCanvasX();
        Vector3 canvasLocation = minimapManager.GetBasePosition();
        ClearElements();
        foreach (Transform child in minimapManager.minimapBase)
        {
            Instantiate(child.gameObject, elementHolder);
        }
        RectTransform elementTransform = (RectTransform)elementHolder.GetChild(0);
        RectTransform rectTransform = (RectTransform)transform;

        minimapMax = rectTransform.sizeDelta.x / elementTransform.sizeDelta.x;
        minimapWorldSize = canvasX;
        maxDistance = ((RectTransform)elementHolder).sizeDelta.x / 2f;
        this.canvasLocation = canvasLocation;
        DisplayZoom();
    }

    public void SetPosition(Vector3 position)
    {
        viewPosition = position;
        DisplayPosition();
    }

    public void SetLerpPosition(Vector3 position)
    {
        lerpPosition = position;
    }

    private void DisplayPosition()
    {
        Vector2 viewPos = new Vector2(canvasLocation.x - viewPosition.x, canvasLocation.z - viewPosition.z);
        elementHolder.localPosition = viewPos * (maxDistance/zoomRadius);
    }

    public void SetRotation(float rotation)
    {
        viewRotation = rotation;
        rotationPivot.rotation = Quaternion.Euler(0f, 0f, viewRotation);
    }

    public void SetZoom(float zoomRadius, bool resetLerp = true)
    {
        if (resetLerp) lerpZoomRadius = zoomRadius;
        this.zoomRadius = zoomRadius;
        DisplayZoom();
    }

    public void SetLerpZoom(float zoomRadius)
    {
        lerpZoomRadius = zoomRadius;
    }

    private void DisplayZoom()
    {
        float newScale = minimapMax/((zoomRadius * 2f)/minimapWorldSize);
        elementHolder.localScale = Vector3.one * newScale;
        DisplayPosition();
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

    public void ClearElements()
    {
        foreach (Transform child in elementHolder)
        {
            Destroy(child.gameObject);
        }
    }
}
