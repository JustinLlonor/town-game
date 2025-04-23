using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackScreen : MonoBehaviour
{
    Image img;
    public TextMeshProUGUI[] texts;
    public GameObject roleStuff;
    public GameObject cover;
    public Transform uiFront;
    public Transform uiFrontHolder;

    private void Awake()
    {
        img = gameObject.GetComponent<Image>();
    }

    public void ShowCover()
    {
        cover.SetActive(true);
    }

    public void HideCover()
    {
        cover.SetActive(false);
    }

    public void HideTexts()
    {
        foreach (var t in texts)
        {
            t.enabled = false;
        }
    }

    public void SetAlpha(float alpha)
    {
        img.color = new Color(0f, 0f, 0f, alpha);
        if (alpha == 0f)
        {
            uiFront.SetParent(uiFrontHolder);
        }
        if (alpha == 1f)
        {
            uiFront.SetParent(transform);
        }
        foreach (var t in texts)
        {
            t.color = new Color(t.color.r, t.color.g, t.color.b, alpha);
        }
    }

    public void StartAlphaTransition(float to, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(TransitionAlpha(to, duration));
    }

    IEnumerator TransitionAlpha(float to, float duration)
    {
        float timer = 0f;
        float initial = img.color.a;

        while (timer < 1f)
        {
            yield return null;
            float newAlpha = Mathf.SmoothStep(initial, to, timer);
            SetAlpha(newAlpha);
            timer += Time.deltaTime * (1 / duration);
        }
        SetAlpha(to);
    }
}
