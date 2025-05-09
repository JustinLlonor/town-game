using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PositionButtonUI : MonoBehaviour
{
    [Range(0f, 100f)]
    public float saturationLevel = 40f;
    [Range(0f, 100f)]
    public float borderValueOffset = 30f;
    public TextMeshProUGUI buttonText;
    public Image buttonImage;
    public RawImage iconImage;
    public Image borderImage;
    public Button button;
    public bool isSelected;

    /// <summary>
    /// Sets the button color based on the schedule block color given
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        float H, S, V;
        // Creates the desaturated button color
        Color.RGBToHSV(color, out H, out S, out V);
        S = Mathf.Clamp(S, 0f, saturationLevel / 100f);
        Color buttonColor = Color.HSVToRGB(H, S, V);
        // Creates the darker border color
        V = Mathf.Clamp01(V -  (borderValueOffset/100f));
        Color borderColor = Color.HSVToRGB(H, S, V);

        buttonImage.color = buttonColor;
        borderImage.color = borderColor;
    }

    public void SetText(string text)
    {
        buttonText.text = text;
    }

    public void SetIcon(Texture icon)
    {
        iconImage.texture = icon;
    }
}
