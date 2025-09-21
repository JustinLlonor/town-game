using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    public RawImage icon;
    public RawImage panel;
    public Color regColor;
    public Color highlightColor;

    public void SetIcon(Texture2D texture)
    {
        if (texture == null)
        {
            icon.enabled = false;
            return;
        }
        icon.enabled = true;
        icon.texture = texture;
    }

    public void SetIcon(Texture texture)
    {
        if (texture == null)
        {
            icon.enabled = false;
            return;
        }
        icon.enabled = true;
        icon.texture = texture;
    }

    public void SetIconItem(Item item)
    {
        Texture2D texture = null;
        if (item != null) texture = item.icon;
        if (texture == null)
        {
            icon.enabled = false;
            return;
        }
        icon.enabled = true;
        icon.texture = texture;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlighted)
        {
            panel.color = highlightColor;
            return;
        }
        panel.color = regColor;
    }

    /**
    public void OnEnable()
    {
        StopAllCoroutines();
        isEquipped = true;
        SetEquipped(false);
    }

    public void SetEquipped(bool equipped)
    {
        if (equipped && !animationStarted)
        {
            isEquipped = true;
            panel.color = equipColor;
            animationStarted = true;
            StartHeightAnimation(equipHeight, equipSpeed, equipCurve);
            StartCoroutine(ResetHeight(hideTimer));
            return;
        }
        if (!equipped && isEquipped)
        {
            isEquipped = false;
            panel.color = unequipColor;
            animationStarted = false;
            StartHeightAnimation(unequipHeight, equipSpeed, equipCurve);
        }
    }

    private void StartHeightAnimation(float newHeight, float speed, AnimationCurve curve)
    {
        if (slotHolder.localPosition.y == newHeight) return;
        StopAllCoroutines();
        StartCoroutine(HeightAnimation(newHeight, speed, curve));
    }

    IEnumerator HeightAnimation(float newHeight, float speed, AnimationCurve curve)
    {
        float progress = 0f;
        float originalHeight = slotHolder.localPosition.y;
        while (progress < 1f)
        {
            progress += Time.deltaTime * speed;
            float eval = curve.Evaluate(progress);
            float foundHeight = Mathf.Lerp(originalHeight, newHeight, eval);
            slotHolder.localPosition = new Vector3(slotHolder.localPosition.x, foundHeight);
            yield return null;
        }
        slotHolder.localPosition = new Vector3(slotHolder.localPosition.x, newHeight);
        animationStarted = false;
    }

    IEnumerator ResetHeight(float duration)
    {
        yield return new WaitForSeconds(duration);
        StartCoroutine(HeightAnimation(unequipHeight, equipSpeed, equipCurve));
    }
    **/
}
