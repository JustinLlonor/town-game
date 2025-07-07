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
        node = info;
        SetStatusText(info.startingValue);
    }

    public void SetStatusText(float value)
    {
        // progress word texct
        float progress = value / 100f;
        int statusWordIndex = Mathf.FloorToInt(node.statusDescriptions.Length * progress);
        if (statusWordIndex == node.statusDescriptions.Length) statusWordIndex--;
        string newText = node.statusDescriptions[statusWordIndex] + " (" + value.ToString("0.0") + "%)";
        statusText.text = newText;
        // attribute colour
        statusText.color = node.statusGradient.Evaluate(progress);
    }
}
