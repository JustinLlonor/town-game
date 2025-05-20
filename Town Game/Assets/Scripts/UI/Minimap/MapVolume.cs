using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Describes UI elements that display certain colors when the map view is inside of the volume
/// </summary>
[System.Serializable]
public class MapVolume
{
    public Collider[] volumeColliders;
    public MaskableGraphic[] associatedGraphics;
    public Color enterColor;
    public Color exitColor;
    public bool fadeOtherElements;
}