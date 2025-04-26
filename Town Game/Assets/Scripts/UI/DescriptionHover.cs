using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DescriptionHover : MonoBehaviour
{
    public float maxWidth = 400f;
    public float verticalPadding = 10f;
    public float horizontalPadding = 5f;
    public string description;
    public PivotPosition verticalPivot;

    public GameObject descriptionObject;
    public TextMeshProUGUI descriptionText;
    public ContentSizeFitter fitter;
    public RectTransform panelRT;
    RectTransform descRT;
    bool previousIsHovering;

    public enum PivotPosition
    {
        Up = 0,
        Middle = 1,
        Down = 2
    }

    private void OnEnable()
    {
        descRT = ((RectTransform)descriptionText.transform);
        previousIsHovering = false;
    }

    void SetPivots()
    {
        float pivotY = (int)verticalPivot * 0.5f;
        panelRT.pivot = new Vector2(0f, pivotY);
        descRT.pivot = new Vector2(0f, pivotY);
    }

    void SetDescriptionText()
    {
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        descriptionText.text = description;
        Canvas.ForceUpdateCanvases();
        float textWidth = descRT.sizeDelta.x;
        if (textWidth > maxWidth)
        {
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            descRT.sizeDelta = new Vector2(maxWidth, descRT.sizeDelta.y);
            Canvas.ForceUpdateCanvases();
        }
        panelRT.sizeDelta = descRT.sizeDelta + new Vector2(horizontalPadding, verticalPadding);
    }

    void SetTextHeight()
    {
        float desiredHeight = descRT.sizeDelta.y / 2f;
        int pivotOffset = (int)verticalPivot - 1;
        float desiredOffset = desiredHeight * pivotOffset;
        descRT.anchoredPosition = new Vector2(descRT.anchoredPosition.x, desiredOffset);
    }

    private void Update()
    {
        bool currentIsHovering = IsHovering();
        if (Input.GetKey(KeyCode.Space)) currentIsHovering = true;
        if (currentIsHovering != previousIsHovering)
        {
            previousIsHovering = currentIsHovering;
            descriptionObject.SetActive(currentIsHovering);
            SetPivots();
            SetDescriptionText();
            SetTextHeight();
        }
    }

    bool IsHovering()
    {
        return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition);
    }
}
