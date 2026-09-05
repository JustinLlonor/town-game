using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[System.Serializable]
public class TextEffect : SceneElement
{
    [Header("Text")]
    public string text;
    public Vector2 anchoredPosition; // Make a game object parent for every possible anchor
    public float textRevealLength;
    public Color color;
    public CutsceneAnchor cutsceneAnchor;
    public CutsceneTextAlignment textAlignment;

    /**
    /// <summary>
    /// Creates a text effect for cutscenes
    /// </summary>
    /// <param name="time"></param>
    /// <param name="length"></param>
    /// <param name="text"></param>
    /// <param name="anchoredPosition">The position of this text element relative to its anchor</param>
    /// <param name="textRevealLength">How long it takes for the entire text to scroll. A value of 0 means the text does not scroll</param>
    /// <param name="color">The color of the text</param>
    /// <param name="cutsceneAnchor">The cutscene anchor of the text effect</param>
    /// <param name="textAlignment">The text alignment of the text effect</param>
    public TextEffect(float time, float length, string text, Vector2 anchoredPosition, float textRevealLength, Color color, 
        CutsceneAnchor cutsceneAnchor = CutsceneAnchor.Middle, CutsceneTextAlignment textAlignment = CutsceneTextAlignment.Middle)
    {
        this.time = time;
        this.length = length;
        this.text = text;
        this.anchoredPosition = anchoredPosition;
        this.textRevealLength = textRevealLength;
        this.color = color;
        this.cutsceneAnchor = cutsceneAnchor;
        this.textAlignment = textAlignment;
    }
    **/
}
