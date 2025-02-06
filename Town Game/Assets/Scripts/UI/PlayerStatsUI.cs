using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    public RectTransform healthTransform;
    public RectTransform nutritionTransform;
    public RectTransform sanityTransform;
    public RectTransform staminaTransform;
    PlayerStats trackedStats;
    float barMax = 0f;
    // stamina stuff
    public MaskableGraphic[] staminaImages;
    public float staminaRevealSpeed = 3f;
    public float staminaLinger = 2f;
    float staminaAlpha = 0f;
    float staminaBarMax = 0f;
    float staminaRevealTimer = 0f;
    float previousStamina;

    private void Awake()
    {
        FindFirstObjectByType<PlayerManager>().OnInstantiatePlayer += AssignPlayerReferences;
        barMax = healthTransform.localScale.y;
        staminaBarMax = staminaTransform.localScale.y;
    }

    void AssignPlayerReferences(GameObject player)
    {
        trackedStats = player.GetComponent<PlayerStats>();
        previousStamina = trackedStats.maxStamina;
    }

    private void Update()
    {
        if (trackedStats == null) return;

        SetBarLengths();
        StaminaReveal();
    }

    void StaminaReveal()
    {
        if (staminaImages[0].enabled == false) return;

        // stamina reveal timer
        // If decreases, set reveal timer to reveal linger
        if (previousStamina != trackedStats.stamina)
        {
            previousStamina = trackedStats.stamina;
            staminaRevealTimer = staminaLinger;
        }
        if (staminaRevealTimer > 0f) staminaRevealTimer -= Time.deltaTime;

        // stamina alpha manipulation
        // If stamina needs to be revealed and the alpha isnt 1
        if (staminaRevealTimer > 0f && staminaAlpha < 1f)
        {
            staminaAlpha += Time.deltaTime * staminaRevealSpeed;
        }
        // If stamina needs to be hidden and alpha isnt 0
        if (staminaRevealTimer <= 0f && staminaAlpha > 0f)
        {
            staminaAlpha -= Time.deltaTime * staminaRevealSpeed;
        }
        staminaAlpha = Mathf.Clamp01(staminaAlpha);
        foreach (MaskableGraphic graphic in staminaImages)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, staminaAlpha);
        }
    }

    void SetBarLengths()
    {
        healthTransform.localScale = new Vector3(healthTransform.localScale.x, barMax * Mathf.Clamp(trackedStats.HP / trackedStats.maxHP, 0f, 1f));
        nutritionTransform.localScale = new Vector3(sanityTransform.localScale.x, barMax * Mathf.Clamp(trackedStats.nutrition / trackedStats.maxNutrition, 0f, 1f));
        sanityTransform.localScale = new Vector3(sanityTransform.localScale.x, barMax * Mathf.Clamp(trackedStats.sanity / trackedStats.maxSanity, 0f, 1f));
        staminaTransform.localScale = new Vector3(staminaTransform.localScale.x, staminaBarMax * Mathf.Clamp(trackedStats.stamina / trackedStats.maxStamina, 0f, 1f));
    }
}
