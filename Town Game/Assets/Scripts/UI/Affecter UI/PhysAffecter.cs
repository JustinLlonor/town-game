using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhysAffecter : MonoBehaviour
{
    public RectTransform outerBorder;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI changeText;
    public float openedHeight;
    public float closedHeight;
    public int decimalPlaces = 2;

    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void SetDescription(string description)
    {
        descriptionText.text = description;
    }

    public void SetColor(Color color)
    {
        changeText.color = color;
    }

    public void SetHeight(float height)
    {
        outerBorder.sizeDelta = new Vector2(outerBorder.sizeDelta.x, height);
    }

    public void SetChange(float changePercent)
    {
        float newChange = Mathf.Round(changePercent * (100 * (10^decimalPlaces)))/(10^decimalPlaces);
        string cText = newChange.ToString() + "%/s";
        if (changePercent < 0f)
        {
            cText = "-" + cText;
        } else
        {
            cText = "+" + cText;
        }
        changeText.text = cText;
    }
}
