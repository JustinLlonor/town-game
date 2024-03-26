using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    public PlayerStats trackedStats;

    [Header("Health Indicator Color")]
    public Gradient healthGradient;
    [Range(0f, 1f)]
    public float flashThreshold = 0.1f;
    public float flashFrequency = 10f;
    public float flashUpperLimit = 1f;
    public float flashLowerLimit = .5f;
    [Header("Health Indicator Speed")]
    public float maxHPSpeed = 0.8f;
    public float lowHPSpeed = 0.4f;
    public float staminaMaxMultiplier = 2f;
    public float staminaMinMultiplier = 0.5f;
    float flashTimer = 0f;

    public Transform healthIndicator;
    private Image blob;

    private void Awake()
    {
        blob = healthIndicator.GetComponent<Image>();
    }

    private void Update()
    {
        if (trackedStats == null) return;
        UpdateBlobColor();
    }

    void UpdateBlobColor()
    {
        float hpPercent = trackedStats.HP / trackedStats.maxHP;
        blob.color = healthGradient.Evaluate(hpPercent);
        if (hpPercent < flashThreshold)
        {
            flashTimer += Time.deltaTime;
            float flashMultiplier = ((Mathf.Sin(flashTimer * flashFrequency) + 1f) * (flashUpperLimit - flashLowerLimit)) / 2f + flashLowerLimit;
            blob.color = new Color(blob.color.r * flashMultiplier, blob.color.g * flashMultiplier, blob.color.b * flashMultiplier);
            return;
        }
        if (flashTimer != 0f) flashTimer = 0f;
    }
}
