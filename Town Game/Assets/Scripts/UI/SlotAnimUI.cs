using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotAnimUI : MonoBehaviour
{
    public SlotUI slotUI;
    public Transform slotHolder;
    public float equipHeight = 0f;
    public float unequipHeight = -100f;
    public AnimationCurve equipCurve;
    public float equipSpeed = 4f;
    public float hideTimer = 5f;
    bool isEquipped = false;
    bool animationStarted = false;

    public void OnEnable()
    {
        StopAllCoroutines();
        isEquipped = true;
        SetEquipped(false);
    }

    public void SetEquipped(bool equipped)
    {
        if (!gameObject.activeInHierarchy) return;
        if (equipped && !animationStarted)
        {
            isEquipped = true;
            //panel.color = equipColor;
            slotUI.SetHighlighted(true);
            animationStarted = true;
            StartHeightAnimation(equipHeight, equipSpeed, equipCurve);
            StartCoroutine(ResetHeight(hideTimer));
            return;
        }
        if (!equipped && isEquipped)
        {
            isEquipped = false;
            slotUI.SetHighlighted(false);
            //panel.color = unequipColor;
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
}
