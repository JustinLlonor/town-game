using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSlotUI : MonoBehaviour
{
    public int slotID;
    public SlotUI slotUI;
    public RectTransform clickTarget;
    public BoolEvent onHoverChange;
    private bool isHovering = false;

    public delegate void BoolEvent(bool hovering, int id);

    private void OnEnable()
    {
        SetHovering(false);
    }

    private void Update()
    {
        SetHovering(MouseHovering());
    }

    private void SetHovering(bool hovering)
    {
        if (isHovering == hovering) return;
        slotUI.SetHighlighted(hovering);
        isHovering = hovering;
        onHoverChange?.Invoke(isHovering, slotID);
    }

    bool MouseHovering()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(clickTarget, Input.mousePosition);
    }
}
