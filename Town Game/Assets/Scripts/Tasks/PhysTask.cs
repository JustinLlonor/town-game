using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PhysTask : MonoBehaviour
{
    public string taskName;
    public TextMeshProUGUI taskText;
    public int percentCharacters = 4;
    float progress;
    public float textOffset;

    public void SetText(string name)
    {
        taskName = name;
        UpdateText();
    }

    public void SetProgress(float newProgress)
    {
        progress = newProgress;
        UpdateText();
    }

    void UpdateText()
    {
        taskText.text = taskName;
        return;
        string newPercent = (progress * 100f).ToString();
        if (newPercent.Length > percentCharacters)
        {
            newPercent = newPercent.Substring(0, percentCharacters);
        }
        taskText.text = $"{taskName} [{newPercent}%]";
    }
}
