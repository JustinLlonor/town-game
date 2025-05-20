using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    /// <summary>
    /// Holds the static UI elements of the minimap. 
    /// Every child is a UI GameObject pertaining to some elevation and bounds 
    /// (The RectTransform sizeDelta determines the bounds, the GameObject name determines elevation).
    /// Any elevation that the viewed location is above or not on is faded into the background of the minimap.
    /// </summary>
    public Transform minimapBase;
    public MapVolume[] mapVolumes = new MapVolume[0];
    public Dictionary<string, MinimapIcon> icons = new Dictionary<string, MinimapIcon>();
    public Dictionary<string, MinimapPointer> pointers = new Dictionary<string, MinimapPointer>(); 
    public IconEvent onIconAdd;
    public IconEvent onIconRemove;
    public IconEvent onIconMove;
    public IconEvent onIconRotate;
    public PointerEvent onPointerAdd;
    public PointerEvent onPointerRemove;
    public PointerEvent onPointerMove;

    public delegate void IconEvent(MinimapIcon icon);
    public delegate void PointerEvent(MinimapPointer pointer);

    /// <summary>
    /// Describes UI elements that display certain colors when the map view is inside of the volume
    /// </summary>
    [System.Serializable]
    public struct MapVolume
    {
        public BoxCollider[] volumeColliders;
        public MaskableGraphic[] associatedGraphics;
        public Color enterColor;
        public Color exitColor;
        public bool fadeOtherElements;
    }

    /// <summary>
    /// Adds a new icon to the minimap
    /// </summary>
    /// <param name="name">The name of the icon</param>
    /// <param name="texture">The image the icon uses</param>
    /// <param name="position">The position, in world space, of the icon</param>
    /// <param name="rotation">The rotation, in world space, of the icon</param>
    /// <param name="usesWorldRotation">If this is true, then the icon will have a defined rotation</param>
    /// <param name="sizeX">The sizeDelta x of the RectTransform of this icon</param>
    /// <param name="sizeY">The sizeDelta x of the RectTransform of this icon</param>
    /// <param name="hoverText"></param>
    public void AddIcon(string name, Texture2D texture, Vector3 position, float rotation = 0f, bool usesWorldRotation = true, float sizeX = 2f, float sizeY = 2f, string hoverText = "")
    {
        if (icons.ContainsKey(name)) return;
        MinimapIcon newIcon = new MinimapIcon(name, texture, position, rotation, new Vector2(sizeX, sizeY), usesWorldRotation, hoverText);
        icons.Add(name, newIcon);
        onIconAdd?.Invoke(newIcon);
    }

    /// <summary>
    /// Removes the icon of the specified name
    /// </summary>
    /// <param name="name"></param>
    public void RemoveIcon(string name)
    {
        if (!icons.ContainsKey(name)) return;
        MinimapIcon removedIcon = icons[name];
        icons.Remove(name);
        onIconRemove?.Invoke(removedIcon);
    }

    public void SetIconPosition(string name, Vector3 position)
    {
        if (!icons.ContainsKey(name)) return;
        MinimapIcon icon = icons[name];
        if (icon.position != position)
        {
            icon.position = position;
            onIconMove?.Invoke(icon);
        }
    }

    public void SetIconRotation(string name, float rotation)
    {
        if (!icons.ContainsKey(name)) return;
        MinimapIcon icon = icons[name];
        if (icon.rotation != rotation)
        {
            icon.rotation = rotation;
            onIconRotate?.Invoke(icon);
        }
    }

    public MinimapIcon GetIcon(string name)
    {
        return icons[name];
    }

    public MinimapIcon[] GetAllIcons()
    {
        MinimapIcon[] output = new MinimapIcon[icons.Count];
        int i = 0;
        foreach (var icon in icons)
        {
            output[i] = icon.Value;
        }
        return output;
    }

    public void AddPointer(string name, Vector3 pointLocation, Color color, bool disappearOnSight = false)
    {
        if (pointers.ContainsKey(name)) return;
        MinimapPointer newPointer = new MinimapPointer(name, pointLocation, color, disappearOnSight);
        pointers.Add(name, newPointer);
        onPointerAdd?.Invoke(newPointer);
    }

    public void RemovePointer(string name)
    {
        if (!pointers.ContainsKey(name)) return;
        MinimapPointer removedPointer = pointers[name];
        pointers.Remove(name);
        onPointerRemove?.Invoke(removedPointer);
    }

    public void MovePointer(string name, Vector3 position)
    {
        if (!pointers.ContainsKey(name)) return;
        MinimapPointer movedPointer = pointers[name];
        movedPointer.position = position;
        onPointerMove?.Invoke(movedPointer);
    }

    public float GetCanvasX()
    {
        return ((RectTransform)minimapBase).sizeDelta.x;
    }

    public Vector3 GetBasePosition()
    {
        return minimapBase.position;
    }
}
