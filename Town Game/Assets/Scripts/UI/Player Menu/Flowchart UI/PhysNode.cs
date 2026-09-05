using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PhysNode : MonoBehaviour
{
    public TextMeshProUGUI attributeText;
    public TextMeshProUGUI statusText;
    NodeInfo node;

    public void Init(NodeInfo info)
    {
        attributeText.text = info.name;
        attributeText.color = info.attributeColor;
        node = info;
        SetStatusText(info.startingValue);
    }

    /// <summary>
    /// Sets the status text
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The name of the current status</returns>
    public string SetStatusText(float value)
    {
        // progress word texct
        float progress = value / 100f;
        int statusWordIndex = Mathf.FloorToInt(node.statusDescriptions.Length * progress);
        if (statusWordIndex == node.statusDescriptions.Length) statusWordIndex--;
        string statusDesc = node.statusDescriptions[statusWordIndex];
        string newText = statusDesc + " (" + Mathf.CeilToInt(value) + "%)";
        statusText.text = newText;
        // attribute colour
        statusText.color = node.statusGradient.Evaluate(progress);
        return statusDesc;
    }
}
