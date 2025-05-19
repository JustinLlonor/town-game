using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [Header("View info")]
    public Vector3 viewPosition;
    public float viewRotation = 0f;
    public float zoomRadius = 5f;
    public MapRoom viewRoom;
    public string trackedIcon;
    [Header("Interaction settings")] // Make a separate component for interactions like panning and zooming, lerping is dealt with there as well
    public bool isPannable; // To be removed
    public bool isZoomable;
    public float minZoomRadius;
    public float maxZoomRadius;
    [Header("References")]
    public GameObject iconObject;
    public Transform elementHolder;
    public Transform[] rotationPivots;
    public Transform rotationPivot;
    public Transform iconHolder;

    private Dictionary<MinimapIcon, RectTransform> activeIcons = new Dictionary<MinimapIcon, RectTransform>();
    private List<MinimapIcon> fixedRotationIcons = new List<MinimapIcon>();
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
    MinimapManager minimapManager;
    bool init = false;

    private void OnEnable()
    {
        if (!init)
        {
            init = true;
            Init();
        }
        minimapManager.onIconAdd += AddIcon;
        minimapManager.onIconRemove += RemoveIcon;
        minimapManager.onIconMove += MoveIcon;
        minimapManager.onIconRotate += RotateIcon;
        CheckUnaddedIcons();
    }

    private void OnDisable()
    {
        minimapManager.onIconAdd -= AddIcon;
        minimapManager.onIconRemove -= RemoveIcon;
        minimapManager.onIconMove -= MoveIcon;
        minimapManager.onIconRotate -= RotateIcon;
    }

    public void Init()
    {
        minimapManager = FindAnyObjectByType<MinimapManager>();
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
        maxDistance = rectTransform.sizeDelta.x / 2f;
        this.canvasLocation = canvasLocation;
        DisplayZoom();
    }

    private void Update()
    {
        foreach (var icon in fixedRotationIcons)
        {
            activeIcons[icon].eulerAngles = new Vector3(0f, 0f, icon.rotation);
        }
    }

    public void SetPosition(Vector3 position)
    {
        viewPosition = position;
        DisplayPosition();
    }

    private void DisplayPosition()
    {
        Vector2 viewPos = new Vector2(canvasLocation.x - viewPosition.x, canvasLocation.z - viewPosition.z) * (maxDistance / zoomRadius);
        elementHolder.localPosition = viewPos;
        iconHolder.localPosition = viewPos;
    }

    public void SetRotation(float rotation)
    {
        viewRotation = rotation;
        foreach (Transform pivot in rotationPivots)
        {
            pivot.rotation = Quaternion.Euler(0f, 0f, viewRotation);
        }
    }

    public void SetZoom(float zoomRadius)
    {
        this.zoomRadius = zoomRadius;
        DisplayZoom();
    }

    private void DisplayZoom()
    {
        float newScale = minimapMax/((zoomRadius * 2f)/minimapWorldSize);
        Vector2 scale = Vector3.one * newScale;
        elementHolder.localScale = scale;
        iconHolder.localScale = scale;
        DisplayPosition();
    }

    private void AddIcon(MinimapIcon icon)
    {
        if (activeIcons.ContainsKey(icon)) return;
        GameObject newIcon = Instantiate(iconObject, iconHolder);
        RectTransform iconTransform = (RectTransform)newIcon.transform;
        // Position and size
        Vector2 iconPos = new Vector2(canvasLocation.x - icon.position.x, canvasLocation.z - icon.position.z) * (maxDistance / zoomRadius);
        iconTransform.localPosition = iconPos;
        iconTransform.sizeDelta = icon.size;
        // Texture
        Texture2D iconTexture = icon.texture;
        newIcon.GetComponent<RawImage>().texture = iconTexture;
        // Rotation
        if (icon.usesWorldRotation)
        {
            iconTransform.localEulerAngles = new Vector3(0, 0, icon.rotation);
        }
        else
        {
            iconTransform.eulerAngles = new Vector3(0, 0, icon.rotation);
            fixedRotationIcons.Add(icon);
        }
        activeIcons.Add(icon, iconTransform);
    }

    private void MoveIcon(MinimapIcon icon)
    {
        if (!activeIcons.ContainsKey(icon))
        {
            AddIcon(icon);
            return;
        }
        Vector2 newPos = new Vector2(icon.position.x - canvasLocation.x, icon.position.z - canvasLocation.z) * ((maxDistance / zoomRadius)/iconHolder.localScale.x);
        activeIcons[icon].localPosition = newPos;
    }

    private void RotateIcon(MinimapIcon icon)
    {
        if (!activeIcons.ContainsKey(icon))
        {
            AddIcon(icon);
            return;
        }
        if (icon.usesWorldRotation)
        {
            activeIcons[icon].localEulerAngles = new Vector3(0, 0, icon.rotation);
            return;
        }
        activeIcons[icon].eulerAngles = new Vector3(0, 0, icon.rotation);
    }

    private void RemoveIcon(MinimapIcon icon)
    {
        if (!activeIcons.ContainsKey(icon)) return;
        Destroy(activeIcons[icon].gameObject);
        if (fixedRotationIcons.Contains(icon)) fixedRotationIcons.Remove(icon);
        activeIcons.Remove(icon);
    }

    private void CheckUnaddedIcons()
    {
        foreach (var kvp in minimapManager.icons)
        {
            if (!activeIcons.ContainsKey(kvp.Value))
            {
                AddIcon(kvp.Value);
            }
        }
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
