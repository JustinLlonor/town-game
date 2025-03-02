using Fusion.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabBlockPhys : MonoBehaviour
{
    public RectTransform manipulatedTransform;
    public RectTransform hitbox;
    public float animationSpeed = 5f;
    public float maxWidth = -1f;
    public float minWidth = -1f;
    bool hovering = false;
    float currentWidth;

    public void Init(float newWidth, float newCurrent, float pivotX, float ySize)
    {
        maxWidth = newWidth;
        currentWidth = newCurrent;
        minWidth = newCurrent;
        hitbox.sizeDelta = new Vector2(currentWidth, ySize - 0.05f);
        hitbox.pivot = new Vector2(pivotX, 1f);
        hitbox.anchorMax = new Vector2(pivotX, hitbox.anchorMax.y);
        hitbox.anchorMin = new Vector2(pivotX, hitbox.anchorMin.y);
        hitbox.localPosition = new Vector2(0f, 0f);
    }

    private void Update()
    {
        if (maxWidth == -1f) return;
        bool currentHover = MouseHovering();
        if (currentHover != hovering)
        {
            hovering = currentHover;
            StopAllCoroutines();
            if (hovering)
            {
                Debug.Log("Starting expansion");
                SetToTopLayer();
                StartCoroutine(Expand(currentWidth, maxWidth));
            } else
            {
                Debug.Log("Ending");
                StartCoroutine(Expand(currentWidth, minWidth));
            }
        }
    }

    IEnumerator Expand(float initialWidth, float endWidth)
    {
        float progress = 0f;
        while (progress < 1f)
        {
            yield return null;
            currentWidth = Mathf.SmoothStep(initialWidth, endWidth, progress);
            manipulatedTransform.sizeDelta = new Vector2(currentWidth, manipulatedTransform.sizeDelta.y);
            progress += Time.deltaTime * animationSpeed;
        }
        manipulatedTransform.sizeDelta = new Vector2(endWidth, manipulatedTransform.sizeDelta.y);
    }

    void SetToTopLayer()
    {
        transform.SetSiblingIndex(transform.parent.childCount-1);
    }

    bool MouseHovering()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(hitbox, Input.mousePosition);
    }

    public void StartExpansion()
    {
        Debug.Log("Starting");
        if (maxWidth ==  -1f) return;
    }

    public void EndExpansion()
    {
        Debug.Log("Ending");
        if (maxWidth == -1f) return;
    }
}
