using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractableUI : MonoBehaviour
{
    public float fillHeight = 37f;
    public float returnLerp = 20f;
    public float alphaLerp = 40f;
    public GameObject interactPrefab;
    public float maxAlpha = .6f;
    Transform interacted = null;
    float iAlpha = 1f;

    private void Awake()
    {
        iAlpha = maxAlpha;
    }

    private void Update()
    {
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
                TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
                text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            }
        }
    }

    public void AddInteraction(string text, Color color)
    {
        GameObject interaction = Instantiate(interactPrefab, transform);
        TextMeshProUGUI tex = interaction.GetComponent<TextMeshProUGUI>();
        tex.text = text;
        tex.color = new Color(color.r, color.g, color.b, maxAlpha);
        interaction.transform.GetChild(0).GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.3f);
    }

    public void ClearInteractions()
    {
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
        if (interacted != null) ((RectTransform)interacted.GetChild(0)).sizeDelta = new Vector2(1700f, 0f);
        interacted = null;
        StopAllCoroutines();
    }

    IEnumerator HighlightAnimation(Transform interaction, float interactionTime)
    {
        float timer = 0f;
        RectTransform img = (RectTransform)interaction.GetChild(0);
        while (timer < interactionTime)
        {
            timer += Time.deltaTime;
            float percent = timer / interactionTime;
            img.sizeDelta = new Vector2(img.sizeDelta.x, percent * fillHeight);
            yield return null;
        }
    }
}
