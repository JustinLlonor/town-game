using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using WebSocketSharp;
using UnityEngine.InputSystem;

public class InteractableUI : MonoBehaviour
{
    public float fillHeight = 37f;
    public float returnLerp = 20f;
    public float alphaLerp = 40f;
    public GameObject interactPrefab;
    public float maxAlpha = .6f;
    public AnimationCurve fillCurve;
    Transform interacted = null;
    float iAlpha = 1f;

    private void Awake()
    {
        iAlpha = maxAlpha;
    }

    private void Update()
    {
        return;
        if (interacted != null)
        {
            if (iAlpha == 0f) return;
            iAlpha = Mathf.Lerp(iAlpha, 0f, alphaLerp * Time.deltaTime);
            SetAlphas(iAlpha);
        }
        else
        {
            if (iAlpha == maxAlpha) return;
            iAlpha = Mathf.Lerp(iAlpha, maxAlpha, alphaLerp * Time.deltaTime);
            SetAlphas(iAlpha);
        }
    }

    void SetAlphas(float alpha)
    {
        foreach (Transform child in transform)
        {
            if (child != interacted)
            {
                TextMeshProUGUI text = child.GetChild(1).GetComponent<TextMeshProUGUI>();
                text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                KeyUI keyUI = GetComponentInChildren<KeyUI>();
                Color keyTextColor = keyUI.keyText.color;
                keyUI.SetKeyColor(new Color(keyTextColor.r, keyTextColor.g, keyTextColor.b, alpha));
                keyUI.SetKeyAlpha(alpha);
            }
        }
    }

    public void AddInteraction(string key, string text, Color color, Color fillColor)
    {
        GameObject interaction = Instantiate(interactPrefab, transform);
        TextMeshProUGUI tex = interaction.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        tex.text = text;
        tex.color = new Color(color.r, color.g, color.b, maxAlpha);
        interaction.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = fillColor;
        KeyUI keyUI = interaction.GetComponentInChildren<KeyUI>();
        if (!key.IsNullOrEmpty())
        {
            keyUI.SetKey(key);
            keyUI.gameObject.SetActive(true);
        }
        else
        {
            keyUI.gameObject.SetActive(false);
        }
        Canvas.ForceUpdateCanvases();
    }


    public void SetInteractionLore(string key, int index, string lore)
    {
        bool refreshCanvas = false;
        Transform interaction = transform.GetChild(index);
        TextMeshProUGUI iText = interaction.GetChild(1).GetComponent<TextMeshProUGUI>();
        if (iText.text != lore)
        {
            iText.text = lore;
            refreshCanvas = true;
        }
        KeyUI keyUI = interaction.GetChild(0).GetComponent<KeyUI>();
        Debug.Log(keyUI);
        if (!key.IsNullOrEmpty())
        {
            if (!keyUI.gameObject.activeSelf) refreshCanvas = true;
            keyUI.SetKey(key);
            keyUI.gameObject.SetActive(true);
        }
        else
        {
            if (keyUI.gameObject.activeSelf) refreshCanvas = true;
            keyUI.gameObject.SetActive(false);
        }
        if (refreshCanvas)
        {
            keyUI.gameObject.SetActive(!keyUI.gameObject.activeSelf);
            Canvas.ForceUpdateCanvases();
            keyUI.gameObject.SetActive(!keyUI.gameObject.activeSelf);
            Canvas.ForceUpdateCanvases();
        }
    }

    public void SetInteractionColor(int index, Color color, Color keyColor)
    {
        Transform interaction = transform.GetChild(index);
        TextMeshProUGUI tmp = interaction.GetChild(1).GetComponent<TextMeshProUGUI>();
        color.a = tmp.color.a;
        tmp.color = color;
        KeyUI keyUI = interaction.GetComponentInChildren<KeyUI>();
        keyUI.SetKeyColor(keyColor);
    }


    public void ClearInteractions()
    {
        StopAllCoroutines();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void StartHighlight(Transform interaction, float interactionTime)
    {
        interacted = interaction;
        StopAllCoroutines();
        StartCoroutine(HighlightAnimation(interaction, interactionTime));
    }

    public void StopHighlight()
    {
        if (interacted != null) ((RectTransform)interacted.GetChild(0).GetChild(0)).sizeDelta = new Vector2(1700f, 0f);
        interacted = null;
        StopAllCoroutines();
    }

    public void SetHighlight(Transform interaction, float percent)
    {
        if (interacted != interaction) interacted = interaction;
        RectTransform img = (RectTransform)interaction.GetChild(0).GetChild(0);
        float eval = fillCurve.Evaluate(percent);
        img.sizeDelta = new Vector2(img.sizeDelta.x, eval * fillHeight);
    }

    IEnumerator HighlightAnimation(Transform interaction, float interactionTime)
    {
        float timer = 0f;
        RectTransform img = (RectTransform)interaction.GetChild(0).GetChild(0);
        while (timer < interactionTime)
        {
            timer += Time.deltaTime;
            float percent = timer / interactionTime;
            float eval = fillCurve.Evaluate(percent);
            img.sizeDelta = new Vector2(img.sizeDelta.x, eval * fillHeight);
            yield return null;
        }
    }
}
