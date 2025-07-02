using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhysTask : MonoBehaviour
{
    [Header("Settings")]
    public float minHeight = 56.35f;
    public float padding = 5f;
    [Header("References")]
    public GameObject completionObject;
    public TextMeshProUGUI taskText;
    public TextMeshProUGUI locationText;
    // The graphics that can change colour with a function
    public MaskableGraphic[] graphics;
    public GameObject cancelObject;

    /// <summary>
    /// Sets the task text and resizes it
    /// </summary>
    /// <param name="text"></param>
    public void SetTaskText(string text)
    {
        taskText.text = text;
        Vector2 preferredValues = taskText.GetPreferredValues();
        float newY = preferredValues.y + padding * 2;
        RectTransform rt = (RectTransform)transform;
        if (newY > minHeight)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, newY);
            return;
        }
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, minHeight);
    }

    /// <summary>
    /// Sets the color of the task's graphics
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        foreach (MaskableGraphic graphic in graphics)
        {
            graphic.color = color;
        }
        taskText.color = color;
        locationText.color = color;
    }

    public void SetCompleted(bool completed)
    {
        completionObject.SetActive(completed);
    }

    public void Cancel()
    {
        cancelObject.SetActive(true);
    }
}
