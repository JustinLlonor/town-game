using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CutsceneTextUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI cursor;
    private string finalText;
    private float currentProgress = -1f;
    private float finishedProgress = -1f;
    bool init = false;
    bool canFlash = false;
    bool flashOn = true;
    float flashProgress = 0f;
    int textLength = 0;

    public void Init(TextEffect effectInfo, float localTime, string processedText)
    {
        text.color = effectInfo.color;
        finishedProgress = effectInfo.textRevealLength;
        currentProgress = effectInfo.GetProgress(localTime) * effectInfo.textRevealLength;
        finalText = processedText;
        textLength = finalText.Length;
        init = true;
    }

    private void Update()
    {
        if (!init) return;
        currentProgress += Time.deltaTime;
        if (currentProgress > finishedProgress) canFlash = true;
        ProcessText();
        Flash();
    }

    private void ProcessText()
    {
        float textProgress = Mathf.Clamp01(currentProgress / finishedProgress);
        int length = Mathf.FloorToInt(textProgress * (textLength-1));
        string outputString = finalText.Substring(0, length);
        text.text = outputString;
    }

    private void Flash()
    {
        if (!canFlash) return;
        flashProgress += Time.deltaTime;
        if (flashProgress > .5f)
        {
            flashOn = !flashOn;
            cursor.enabled = flashOn;
            flashProgress -= .5f;
        }
    }
}
