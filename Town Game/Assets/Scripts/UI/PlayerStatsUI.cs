using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

public class PlayerStatsUI : MonoBehaviour
{
    public RectTransform healthTransform;
    public RectTransform nutritionTransform;
    public RectTransform sanityTransform;
    PlayerStats trackedStats;
    float barMax = 0f;

    private void Awake()
    {
        FindObjectOfType<PlayerManager>().OnInstantiatePlayer += AssignPlayerReferences;
        barMax = healthTransform.localScale.y;
    }

    void AssignPlayerReferences(GameObject player)
    {
        trackedStats = player.GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (trackedStats == null) return;

        healthTransform.localScale = new Vector3(healthTransform.localScale.x, barMax * Mathf.Clamp(trackedStats.HP / trackedStats.maxHP, 0f, 1f));
        nutritionTransform.localScale = new Vector3(sanityTransform.localScale.x, barMax * Mathf.Clamp(trackedStats.nutrition / trackedStats.maxNutrition, 0f, 1f));
        sanityTransform.localScale = new Vector3(sanityTransform.localScale.x, barMax * Mathf.Clamp(trackedStats.sanity / trackedStats.maxSanity, 0f, 1f));
    }
}
