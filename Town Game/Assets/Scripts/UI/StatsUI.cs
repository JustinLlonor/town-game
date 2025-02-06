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
    [Header("Splatter")]
    public AnimationCurve splatterSizeDistribution;
    public float minSplatterSize;
    public float maxSplatterSize;
    public int splatterAmount;
    public float velocityDivider = 2f;
    public float shrinkDivider = 3f;
    public Transform splatterHolder;
    public GameObject splatterPrefab;
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
        PlayerManager pm = FindFirstObjectByType<PlayerManager>();
        pm.OnInstantiatePlayer += AssignPlayerReferences;
    }

    private void Start()
    {
        blob = healthIndicator.GetComponent<Image>();
        staminaMax = staminaBarTransform.localScale.x;
        originalHPPos = healthIndicator.localPosition;
    }

    private void Update()
    {
        if (trackedStats == null) return;
        UpdateBlobColor();
        UpdateBlobSpeed();
        UpdateStaminaBar();
        ResetShakePos();
    }

    void AssignPlayerReferences(GameObject player)
    {
        Debug.Log("assignign plaginyoer");
        trackedStats = player.GetComponent<PlayerStats>();
        trackedStats.OnTakeDamage += ShakeBlob;
        trackedStats.OnDeath += Splatter;
        trackedStats.OnDeath += HideBlob;
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
        Vector3 shakeDirection = Random.insideUnitCircle.normalized;
        healthIndicator.localPosition = healthIndicator.localPosition + shakeDirection * damageShake * (damage / 30f);
    }

    void HideBlob()
    {
        healthIndicator.gameObject.SetActive(false);
    }

    void Splatter()
    {
        for (int i = 0; i < splatterAmount; i++)
        {
            GameObject droplet = Instantiate(splatterPrefab, splatterHolder);

            // Randomize droplet size
            float sizeEval = (float)i / (float)splatterAmount;
            float size = splatterSizeDistribution.Evaluate(sizeEval) * (maxSplatterSize - minSplatterSize) + minSplatterSize;
            droplet.transform.localScale = Vector3.one * size;

            UISplatter uSplatter = droplet.GetComponent<UISplatter>();

            uSplatter.direction = Random.insideUnitCircle;
            uSplatter.velocity = velocityDivider / size;
            uSplatter.shrinkSpeed = shrinkDivider / size;
        }
    }
}
