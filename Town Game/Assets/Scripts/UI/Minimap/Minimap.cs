using Fusion;
using System.Collections.Generic;
using UnityEditor;
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
    public float pointViewPadding = .2f;
    [Header("Interaction settings")] // Make a separate component for interactions like panning and zooming, lerping is dealt with there as well
    public bool isPannable; // To be removed
    public bool isZoomable;
    public float minZoomRadius;
    public float maxZoomRadius;
    [Header("Settings")]
    public float colorTransitionSpeed = 3f;
    public AnimationCurve transitionCurve;
    public LayerMask mapVolumeMask;
    [Header("References")]
    public GameObject iconObject;
    public GameObject pointerObject;
    public Transform elementHolder;
    public Transform[] rotationPivots;
    public Transform rotationPivot;
    public Transform iconHolder;
    public Transform pointerHolder;

    private Dictionary<MinimapIcon, RectTransform> activeIcons = new Dictionary<MinimapIcon, RectTransform>();
    private List<MinimapIcon> fixedRotationIcons = new List<MinimapIcon>();

    private Dictionary<MinimapPointer, RectTransform> activePointers = new Dictionary<MinimapPointer, RectTransform>();

    // yeah it's spaghetti. but i do not care because i like having my game finished
    MapVolume currentMapVolume;
    private Dictionary<MapVolume, List<MaskableGraphic>> localMaskables = new Dictionary<MapVolume, List<MaskableGraphic>>();
    private Dictionary<MaskableGraphic, MapVolume> maskableVolumes = new Dictionary<MaskableGraphic, MapVolume>();
    private List<DefaultGraphic> allGraphics = new List<DefaultGraphic>();
    private List<ColorTransition> colorTransitions = new List<ColorTransition>();

    private class ColorTransition
    {
        public MaskableGraphic graphic;
        public Color fromColor;
        public Color toColor;
        public float progress;

        public ColorTransition(MaskableGraphic graphic, Color fromColor, Color toColor)
        {
            this.graphic = graphic;
            this.fromColor = fromColor;
            this.toColor = toColor;
            progress = 0f;
        }
    }

    private class DefaultGraphic
    {
        public MaskableGraphic graphic;
        public Color originalColor;
        public Color enterColor;

        public DefaultGraphic(MaskableGraphic graphic, Color originalColor, Color enterColor)
        {
            this.graphic = graphic;
            this.originalColor = originalColor;
            this.enterColor = enterColor;
        }
    }

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
        minimapManager.onPointerAdd += AddPointer;
        minimapManager.onPointerRemove += RemovePointer;
        CheckUnaddedElements();
        CheckUnremovedElements();
        SetPositions();
        CheckMapVolume();
    }

    private void OnDisable()
    {
        minimapManager.onIconAdd -= AddIcon;
        minimapManager.onIconRemove -= RemoveIcon;
        minimapManager.onIconMove -= MoveIcon;
        minimapManager.onIconRotate -= RotateIcon;
        minimapManager.onPointerAdd -= AddPointer;
        minimapManager.onPointerRemove -= RemovePointer;
    }

    private void Init()
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
        GetReferences();
        DisplayZoom();
    }

    private void Update()
    {
        CheckTransitions();
    }

    private void LateUpdate()
    {
        FixIconRotation(); // if this glitches, put htis in regular Update method
        foreach (var pointer in activePointers.Keys)
        {
            MovePointer(pointer);
        }
    }

    public void SetPosition(Vector3 position)
    {
        if (!init)
        {
            init = true;
            Init();
        }
        viewPosition = position;
        DisplayPosition();
    }

    private void DisplayPosition()
    {
        Vector2 viewPos = new Vector2(canvasLocation.x - viewPosition.x, canvasLocation.z - viewPosition.z) * (maxDistance / zoomRadius);
        elementHolder.localPosition = viewPos;
        iconHolder.localPosition = viewPos;
        pointerHolder.localPosition = viewPos;
        CheckMapVolume();
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
        pointerHolder.localScale = scale;
        DisplayPosition();
    }

    private void AddIcon(MinimapIcon icon)
    {
        if (activeIcons.ContainsKey(icon)) return;
        GameObject newIcon = Instantiate(iconObject, iconHolder);
        RectTransform iconTransform = (RectTransform)newIcon.transform;
        // Position and size
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
        MoveIcon(icon);
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
            activeIcons[icon].localEulerAngles = new Vector3(0, 0, -icon.rotation);
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

    private void CheckUnaddedElements()
    {
        foreach (var kvp in minimapManager.icons)
        {
            if (!activeIcons.ContainsKey(kvp.Value))
            {
                AddIcon(kvp.Value);
            }
        }
        foreach (var kvp in minimapManager.pointers)
        {
            if (!activePointers.ContainsKey(kvp.Value))
            {
                AddPointer(kvp.Value);
            }
        }
    }

    private void CheckUnremovedElements()
    {
        List<MinimapIcon> removedIcons = new List<MinimapIcon>();
        foreach (var kvp in activeIcons)
        {
            if (!minimapManager.icons.ContainsKey(kvp.Key.name))
            {
                removedIcons.Add(kvp.Key);
            }
        }
        foreach (MinimapIcon icon in removedIcons) RemoveIcon(icon);
        List<MinimapPointer> removedPointers = new List<MinimapPointer>();
        foreach (var kvp in activePointers)
        {
            if (!minimapManager.pointers.ContainsKey(kvp.Key.name))
            {
                removedPointers.Add(kvp.Key);
            }
        }
        foreach (MinimapPointer pointer in removedPointers) RemovePointer(pointer);
    }

    private void SetPositions()
    {
        foreach (var kvp in minimapManager.icons)
        {
            MinimapIcon icon = kvp.Value;
            MoveIcon(icon);
            RotateIcon(icon);
        }
    }

    /// <summary>
    /// Note (to be deleted later) pointers that point to a higher elevation will oscillate between normal size and larger for an animation,
    /// pointers that point to a lower elevation will oscillate between normal and smaller for an animation. Both happen somewhat slowly and subtly
    /// </summary>
    /// <param name="pointLocation"></param>
    /// <param name="color"></param>
    public void AddPointer(MinimapPointer pointer)
    {
        if (activePointers.ContainsKey(pointer)) return;
        GameObject newPointer = Instantiate(pointerObject, pointerHolder);
        RectTransform pointerTransform = (RectTransform)newPointer.transform;
        pointerTransform.GetComponentInChildren<RawImage>().color = pointer.color;
        activePointers.Add(pointer, pointerTransform);
        MovePointer(pointer);
    }

    public void RemovePointer(MinimapPointer pointer)
    {
        if (!activePointers.ContainsKey(pointer)) return;
        Destroy(activePointers[pointer].gameObject);
        activePointers.Remove(pointer);
    }

    public void MovePointer(MinimapPointer pointer)
    {
        if (!activePointers.ContainsKey(pointer))
        {
            AddPointer(pointer);
            return;
        }
        float sightDistance = zoomRadius - pointViewPadding;
        Vector2 pointPos = new Vector2(pointer.position.x, pointer.position.z);
        Vector2 viewPos = new Vector2(viewPosition.x, viewPosition.z);
        Vector2 viewToPoint = pointPos - viewPos;
        Vector2 placedPosition = Vector2.ClampMagnitude(viewToPoint, sightDistance) + viewPos;
        RectTransform activePointer = activePointers[pointer];
        Vector2 newPos = new Vector2(placedPosition.x - canvasLocation.x, placedPosition.y - canvasLocation.z) * ((maxDistance / zoomRadius) / pointerHolder.localScale.x);
        activePointer.localPosition = newPos;
        activePointer.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(viewToPoint.y, viewToPoint.x) * Mathf.Rad2Deg + viewRotation - 90f);
    }

    public void ClearElements()
    {
        foreach (Transform child in elementHolder)
        {
            Destroy(child.gameObject);
        }
    }

    private void CheckMapVolume()
    {
        MapVolume newVolume = GetCurrentVolume();
        if (newVolume == currentMapVolume) return;
        currentMapVolume = newVolume;
        RecalculateTransitions();
    }

    private MapVolume GetCurrentVolume()
    {
        foreach (MapVolume volume in minimapManager.mapVolumes)
        {
            foreach (Collider collider in volume.volumeColliders)
            {
                if (ColliderContainsPoint(collider, viewPosition))
                {
                    return volume;
                }
            }
        }
        return null;
    }

    private bool ColliderContainsPoint(Collider collider, Vector3 worldPosition)
    {
        var direction = collider.bounds.center - worldPosition;
        var ray = new Ray(worldPosition, direction);

        var contains = collider.Raycast(ray, out var hit, direction.magnitude);

        return !contains;
    }

    private void GetReferences()
    {
        maskableVolumes.Clear();
        localMaskables.Clear();
        // Create maskable volumes
        foreach (MapVolume volume in minimapManager.mapVolumes)
        {
            foreach (MapVolume.AssociatedGraphic aGraphic in volume.associatedGraphics)
            {
                maskableVolumes.Add(aGraphic.graphic, volume);
            }
        }
        SearchDepth(minimapManager.minimapBase, new List<int>() { });
    }

    /// <summary>
    /// Searches the transform and its children recursively for a MapVolume corresponding to its MaskableGraphic component (if it has one).
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="coords"></param>
    private void SearchDepth(Transform transformSearch, List<int> coords)
    {
        MaskableGraphic transformGraphic = transformSearch.GetComponent<MaskableGraphic>();
        if (transformGraphic != null)
        {
            // If maskable volumes contains some corresponding map volume, add this maskable to that map volume
            Transform localMaskableTransform = GetTransformFromCoords(coords);
            MaskableGraphic localGraphic = localMaskableTransform.GetComponent<MaskableGraphic>();
            if (maskableVolumes.ContainsKey(transformGraphic))
            {
                if (localGraphic != null)
                {
                    allGraphics.Add(new DefaultGraphic(localGraphic, localGraphic.color, maskableVolumes[transformGraphic].GetEnterColor(transformGraphic)));
                    AddLocalGraphicToVolume(maskableVolumes[transformGraphic], localGraphic);
                }
                else
                {
                    Debug.LogError("Something went wrong when trying to add a local graphic to a MapVolume!");
                }
            }
            else
            {
                if (localGraphic != null)
                {
                    allGraphics.Add(new DefaultGraphic(localGraphic, localGraphic.color, Color.clear));
                }
            }
        }

        int childIndex = 0;
        foreach (Transform child in transformSearch)
        {
            List<int> newList = new List<int>(coords);
            newList.Add(childIndex);
            SearchDepth(child, newList);
            childIndex++;
        }
    }

    private Transform GetTransformFromCoords(List<int> coords)
    {
        Transform pointerTransform = elementHolder;
        for (int i = 0; i < coords.Count; i++)
        {
            pointerTransform = pointerTransform.GetChild(coords[i]);
        }
        return pointerTransform;
    }

    private void AddLocalGraphicToVolume(MapVolume volume, MaskableGraphic graphic)
    {
        if (localMaskables.ContainsKey(volume))
        {
            localMaskables[volume].Add(graphic);
            return;
        }
        localMaskables.Add(volume, new List<MaskableGraphic>() { graphic });
    }

    private void FixIconRotation()
    {
        foreach (var icon in fixedRotationIcons)
        {
            activeIcons[icon].eulerAngles = new Vector3(0f, 0f, icon.rotation);
        }
    }

    private void CheckTransitions()
    {
        for (int i = 0; i < colorTransitions.Count; i++)
        {
            ColorTransition transition = colorTransitions[i];
            transition.progress += Time.deltaTime * colorTransitionSpeed;
            Color newColor = Color.Lerp(transition.fromColor, transition.toColor, transitionCurve.Evaluate(transition.progress));
            transition.graphic.color = newColor;
            if (transition.progress >= 1f)
            {
                colorTransitions.Remove(transition);
                i--;
            }
        }
    }

    private void RecalculateTransitions()
    {
        colorTransitions.Clear();
        if (currentMapVolume == null)
        {
            foreach (DefaultGraphic dGraphic in allGraphics)
            {
                // If we are not our original color, transition back to it
                if (!dGraphic.originalColor.Equals(dGraphic.graphic.color))
                {
                    colorTransitions.Add(new ColorTransition(dGraphic.graphic, dGraphic.graphic.color, dGraphic.originalColor));
                }
            }
            return;
        }

        // Maskables associated with this map volume
        List<MaskableGraphic> locals = localMaskables[currentMapVolume];

        // Background graphics
        foreach (DefaultGraphic dGraphic in allGraphics)
        {
            if (locals.Contains(dGraphic.graphic))
            {
                colorTransitions.Add(new ColorTransition(dGraphic.graphic, dGraphic.graphic.color, dGraphic.enterColor));
                continue;
            }
            if (currentMapVolume.fadeOtherElements)
            {
                if (!dGraphic.graphic.color.Equals(minimapManager.backgroundColor))
                {
                    colorTransitions.Add(new ColorTransition(dGraphic.graphic, dGraphic.graphic.color, minimapManager.backgroundColor));
                }
                continue;
            }
            if (!dGraphic.originalColor.Equals(dGraphic.graphic.color))
            {
                colorTransitions.Add(new ColorTransition(dGraphic.graphic, dGraphic.graphic.color, dGraphic.originalColor));
            }
        }
    }
}
