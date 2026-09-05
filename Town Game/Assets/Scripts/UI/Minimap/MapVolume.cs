using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Describes UI elements that display certain colors when the map view is inside of the volume
/// </summary>
[System.Serializable]
public class MapVolume
{
    public Collider[] volumeColliders;
    public AssociatedGraphic[] associatedGraphics;
    public bool fadeOtherElements;
    Dictionary<MaskableGraphic, Color> graphicEnterColors = null;

    public Color GetEnterColor(MaskableGraphic graphic)
    {
        if (graphicEnterColors == null)
        {
            graphicEnterColors = new Dictionary<MaskableGraphic, Color>();
            foreach (AssociatedGraphic aGraphic in associatedGraphics)
            {
                graphicEnterColors.Add(aGraphic.graphic, aGraphic.enterColor);
            }
        }
        return graphicEnterColors[graphic];
    }

    [System.Serializable]
    public class AssociatedGraphic
    {
        public MaskableGraphic graphic;
        public Color enterColor;
    }
}