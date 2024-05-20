using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhysAffecter : MonoBehaviour
{
    public RectTransform outerBorder;
    public RectTransform bar;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI changeText;
    public float openedHeight;
    public float closedHeight;
    public int decimalPlaces = 2;
    public float maxTime;
    public float time;
    public bool timeAffected = true;
    float maxX;

    private void Awake()
    {
        maxX = bar.sizeDelta.x;
        if (!timeAffected)
        {
            bar.sizeDelta = new Vector2(0f, bar.sizeDelta.y);
        }
    }

    private void Update()
    {
        ProgressBar();
    }

    void ProgressBar()
    {
        bar.sizeDelta = new Vector2((time / maxTime) * maxX, bar.sizeDelta.y);
        time -= Time.deltaTime;
    }

    public void StartTimer(float amount)
    {
        time = amount;
        maxTime = amount;
    }

    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void SetDescription(string description)
    {
        descriptionText.text = description;
    }

    public void SetColor(Color color)
    {
        changeText.color = color;
    }

    public void SetHeight(float height)
    {
        outerBorder.sizeDelta = new Vector2(outerBorder.sizeDelta.x, height);
    }

    public void SetChange(float changePercent)
    {
        float newChange = Mathf.Round(changePercent * (100 * (10^decimalPlaces)))/(10^decimalPlaces);
        string cText = newChange.ToString() + "%/s";
        if (changePercent > 0f)
        {
            cText = "+" + cText;
        }
        changeText.text = cText;
    }
}
