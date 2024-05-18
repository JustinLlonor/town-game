using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PhysTask : MonoBehaviour
{
    public string taskName;
    public TextMeshProUGUI taskText;
    float progress;

    public void SetTask(string name)
    {
        taskName = name;
    }

    public void SetProgress(float newProgress)
    {
        progress = newProgress;
        UpdateText();
    }

    void UpdateText()
    {
        taskText.text = $"{taskName} [{Mathf.RoundToInt(progress * 100f)}%]";
    }
}
