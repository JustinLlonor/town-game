using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractableUI : MonoBehaviour
{
    public float fillHeight = 37f;
    public float returnLerp = 20f;
    public float alphaLerp = 40f;
    public GameObject interactPrefab;
    Transform interacted = null;
    float iAlpha = 1f;

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
            if (iAlpha == 1f) return;
            iAlpha = Mathf.Lerp(iAlpha, 1f, alphaLerp * Time.deltaTime);
            SetAlphas(iAlpha);
        }
    }

    void SetAlphas(float alpha)
    {
        foreach (Transform child in transform)
        {
            if (child != interacted)
            {
                child.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, alpha);
            }
        }
    }

    public void AddInteraction(string text)
    {
        GameObject interaction = Instantiate(interactPrefab, transform);
        interaction.GetComponent<TextMeshProUGUI>().text = text;
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
