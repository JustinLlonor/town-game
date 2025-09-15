using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [Header("If enabled, the alpha value is a percentage of the original alpha of each maskable graphic")]
    public bool useOriginalAlpha;
    private Dictionary<MaskableGraphic, float> originalAlpha = new Dictionary<MaskableGraphic, float>();

    private void Awake()
    {
        if (useOriginalAlpha)
        {
            foreach (MaskableGraphic graphic in graphics)
            {
                originalAlpha.Add(graphic, graphic.color.a);
            }
        }
    }

    private void Update()
    {
        if (!checkAlpha) return;
        if (!useOriginalAlpha)
        {
            if (alpha != previousAlpha)
            {
                previousAlpha = alpha;
                SetGraphics();
            }
            return;
        }
        if (alpha != previousAlpha)
        {
            previousAlpha = alpha;
            SetAlphaPercent(alpha);
        }
    }

    /// <summary>
    /// Sets the alpha of each graphic to a percent of what it originally aws
    /// </summary>
    /// <param name="percent"></param>
    public void SetAlphaPercent(float percent)
    {
        if (!useOriginalAlpha) return;
        Debug.Log("setting alpha percent");
        foreach (MaskableGraphic graphic in graphics)
        {
            if (!originalAlpha.ContainsKey(graphic)) continue;
            Debug.Log("setting");
            float newAlpha = percent * originalAlpha[graphic];
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, newAlpha);
        }
        foreach (TextMeshProUGUI text in texts) // Sets text without the alpha percent feature (yet)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, percent);
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
