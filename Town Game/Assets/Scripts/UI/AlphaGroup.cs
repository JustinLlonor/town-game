using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows a UI object's maskables alpha to be set
/// </summary>
public class AlphaGroup : MonoBehaviour
{
    public bool checkAlpha = false;
    public float alpha = 1f;
    private float previousAlpha = 1f;
    public MaskableGraphic[] graphics;
    public TextMeshProUGUI[] texts;

    private void Update()
    {
        if (!checkAlpha) return;
        if (alpha != previousAlpha)
        {
            previousAlpha = alpha;
            SetGraphics();
        }
    }

    public void SetAlpha(float newAlpha)
    {
        alpha = newAlpha;
        SetGraphics();
    }

    private void SetGraphics()
    {
        foreach (MaskableGraphic graphic in graphics)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
        }
        foreach (TextMeshProUGUI text in texts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
        }
    }
}
