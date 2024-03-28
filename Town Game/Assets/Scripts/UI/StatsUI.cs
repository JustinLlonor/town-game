using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{

    [Header("Health Indicator")]
    public Gradient healthGradient;
    [Range(0f, 1f)]
    public float flashThreshold = 0.1f;
    public float flashFrequency = 10f;
    public float flashUpperLimit = 1f;
    public float flashLowerLimit = .5f;
    public float maxHPSpeed = 0.6f;
    public float lowHPSpeed = 2f;
    public float damageShake = .1f;
    public float shakeSnap = 8f;
    float flashTimer = 0f;
    [Header("References")]
    public PlayerStats trackedStats;
    public Animator blobAnimator;
    public RectTransform staminaBarTransform;
    public Transform healthIndicator;

    private Image blob;
    private Vector3 originalHPPos;

    float staminaMax = 0f;

    private void Awake()
    {
        blob = healthIndicator.GetComponent<Image>();
        staminaMax = staminaBarTransform.localScale.x;
        originalHPPos = healthIndicator.localPosition;
        trackedStats.onDamage += ShakeBlob;
    }

    private void Update()
    {
        if (trackedStats == null) return;
        UpdateBlobColor();
        UpdateBlobSpeed();
        UpdateStaminaBar();
        ResetShakePos();
    }

    void UpdateBlobSpeed()
    {
        float hpPercent = trackedStats.HP / trackedStats.maxHP;
        float tweenMultiplier = lowHPSpeed - maxHPSpeed;
        blobAnimator.SetFloat("speed", tweenMultiplier * hpPercent + maxHPSpeed);
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

    void UpdateStaminaBar()
    {
        float staminaPercent = trackedStats.stamina / trackedStats.maxStamina;
        staminaBarTransform.localScale = new Vector3(Mathf.SmoothStep(0f, staminaMax, staminaPercent), staminaBarTransform.localScale.y);
    }

    void ResetShakePos()
    {
        if (healthIndicator.localPosition != originalHPPos)
        {
            healthIndicator.localPosition = Vector3.Lerp(healthIndicator.localPosition, originalHPPos, Time.deltaTime * shakeSnap);
        }
    }

    void ShakeBlob(float damage)
    {
        Vector3 shakeDirection = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f)).normalized;
        Debug.Log(damage);
        healthIndicator.localPosition = healthIndicator.localPosition + shakeDirection * damageShake * (damage/30f);
    }
}
