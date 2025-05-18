using System.Collections;
using System.Collections.Generic;
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
    public MapElement[] mapElements = new MapElement[0];
    public IconEvent onIconAdd;
    public IconEvent onIconRemove;
    public IconMoveEvent onIconMove;

    public delegate void IconEvent(string iconName);
    public delegate void IconMoveEvent(string iconName, Vector3 newPosition);

    /// <summary>
    /// When the player is in the referenced room, the enter color shows for the associated graphics.
    /// When the player is not in the room, the exit color shows.
    /// This does not apply when the player is viewing a different elevation.
    /// </summary>
    [System.Serializable]
    public struct MapElement
    {
        public MapRoom room;
        public MaskableGraphic[] associatedGraphics;
        public Color enterColor;
        public Color exitColor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">The name of the icon</param>
    /// <param name="texture">The image the icon uses</param>
    /// <param name="position">The position, in world space, of the icon</param>
    /// <param name="rotation">The rotation, in world space, of the icon</param>
    /// <param name="usesWorldRotation">If this is true, then the icon will have a defined rotation</param>
    /// <param name="hoverText"></param>
    public void AddIcon(string name, Texture2D texture, Vector3 position, float rotation, bool usesWorldRotation = true, string hoverText = "")
    {

    }

    public void RemoveIcon(string name)
    {

    }

    public void SetIconPosition(string name, Vector3 position)
    {

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
