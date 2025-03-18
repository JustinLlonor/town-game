using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIGlitch : MonoBehaviour
{
    [Header("Render Texture Effect Settings")]
    public Transform renderTextureHolder;
    public float textureOffsetFrequency = 5f; // Times per second a texture gets offset
    public float maxTextureOffsetDistance = 10f; // Maximum offset
    float offsetTimer;
    int renderTextureIndex;
    [Header("Text Effect Settings")]
    public Transform textHolder;
    public float textOffsetFrequency = 4f; // Times per second a text gets offset
    public float maxTextOffsetDistance = 60f; // Maximum offset
    public string text = "ATTACK";
    float textOffsetTimer;
    int textIndex = 0;

    private void OnEnable()
    {
        foreach (Transform child in textHolder)
        {
            child.GetComponent<TextMeshProUGUI>().text = text;
        }
    }

    private void Update()
    {
        TextureOffset();
        TextOffset();
    }

    void TextureOffset()
    {
        offsetTimer += Time.deltaTime * textureOffsetFrequency;
        if (offsetTimer < 1f) return;
        offsetTimer = 0f;
        Transform childTransform = renderTextureHolder.GetChild(renderTextureIndex);
        Vector2 offset = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * maxTextureOffsetDistance;
        childTransform.localPosition = offset;
        renderTextureIndex++;
        if (renderTextureIndex >= renderTextureHolder.childCount)
        {
            renderTextureIndex = 0;
        }
    }

    void TextOffset()
    {
        textOffsetTimer += Time.deltaTime * textOffsetFrequency;
        if (textOffsetTimer < 1f) return;
        textOffsetTimer = 0f;
        Transform childTransform = textHolder.GetChild(textIndex);
        Vector2 offset = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * maxTextOffsetDistance;
        childTransform.localPosition = offset;
        textIndex++;
        if (textIndex >= textHolder.childCount)
        {
            textIndex = 0;
        }
    }

    public void SetTextCenterPosition(Vector2 screenPosition)
    {
        ((RectTransform)textHolder).position = screenPosition;
    }
}
