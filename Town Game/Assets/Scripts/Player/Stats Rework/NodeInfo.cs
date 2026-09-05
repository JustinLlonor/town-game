using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeInfo
{
    public string name;
    [TextArea(3, 10)]
    public string description;
    [Range(0f, 100f)]
    public float startingValue;
    [Tooltip("The range in which this node will be displayed in the hotbar always")]
    public Vector2 criticalDisplayRange = new Vector2(0f, 25f);
    [Tooltip("The rate of change in units/period. Max units is 100")]
    public float startingRate;
    public bool highIsGood = true;
    public Color attributeColor;
    [Tooltip("The status descriptions that appear at every level of this node")]
    public string[] statusDescriptions = new string[] { "Low", "Moderate", "High" };
    public Gradient statusGradient;
    [Tooltip("If this node gets removed when the value hits 0%")]
    public bool destroyOnZero = false;
}
