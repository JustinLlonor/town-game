using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarStatsUI : MonoBehaviour
{
    public List<Transform> slotHolders = new List<Transform>();
    public Transform hotbarTransform;
    public bool appendToHighestSlot = true;
    public float minHeight = 50f;
    public float heightThreshold = -43f;
    public BarUI healthBar;
    public BarUI hungerBar;
    PlayerStats trackedStats;
    float shMinHeight;
    bool init = false;
    float hotbarScale;

    private void Awake()
    {
        FindFirstObjectByType<PlayerManager>().onInstantiatePlayer += AssignPlayerReferences;
    }

    private void LateUpdate()
    {
        SetHeight();
    }

    void AssignPlayerReferences(GameObject player)
    {   
        trackedStats = player.GetComponent<PlayerStats>();
        healthBar.Init(trackedStats.maxHP);
        hungerBar.Init(trackedStats.maxHunger);
        trackedStats.onHPChangeClient += OnHPChange;
        trackedStats.onHungerChangeClient += OnHungerChange;
    }

    private void OnHPChange(int value)
    {
        healthBar.SetValue(trackedStats.HP);
    }

    private void OnHungerChange(int value)
    {
        hungerBar.SetValue(trackedStats.hunger);
    }

    private void SetHeight()
    {
        if (!appendToHighestSlot) return;
        // Highest height minus the minimum height for the slot holder to get the offset, added to the min h eight of the bar UI
        float newHeight = (GetHighestHeight() - shMinHeight) + minHeight + heightThreshold;
        if (newHeight < minHeight) newHeight = minHeight;
        ((RectTransform)transform).anchoredPosition = new Vector2(0, newHeight);
    }

    public void InitializeSlotHolders()
    {
        init = true;
        bool gotMinHeight = false;
        hotbarScale = hotbarTransform.localScale.x;
        foreach (Transform child in hotbarTransform)
        {
            if (!gotMinHeight)
            {
                gotMinHeight = true;
                shMinHeight = child.GetComponent<SlotUI>().unequipHeight * hotbarScale;
            }
            slotHolders.Add(child.GetComponent<SlotUI>().slotHolder);
        }
    }
    
    /// <summary>
    /// Gets the highest y position of a slot holder
    /// </summary>
    /// <returns></returns>
    public float GetHighestHeight()
    {
        float heighestHeight = shMinHeight;
        foreach (Transform holder in slotHolders)
        {
            float anchoredPos = ((RectTransform)holder).anchoredPosition.y * hotbarScale;
            if (anchoredPos > heighestHeight)
            {
                heighestHeight = anchoredPos;
            }
        }
        return heighestHeight;
    }
}
