using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarStatsUI : MonoBehaviour
{
    [Header("Hotbar Snap Settings")]
    public List<Transform> slotHolders = new List<Transform>();
    public Transform hotbarTransform;
    public bool appendToHighestSlot = true;
    public float minHeight = 50f;
    public float heightThreshold = -43f;
    [Header("Bar Settings")]
    public float barSpacing = 165f;
    public float barSpaceSpeed = 5f;
    public BarUI healthBar;
    public BarUI hungerBar;
    private List<BarUI> activeBars = new List<BarUI>();
    PlayerStats trackedStats;
    float shMinHeight;
    bool init = false;
    float hotbarScale;

    private void Awake()
    {
        //FindFirstObjectByType<PlayerManager>().onInstantiatePlayer += AssignPlayerReferences;
    }

    private void LateUpdate()
    {
        SetHeight();
        CheckBars();
        AdjustBarSpacing();
    }

    /**
    void AssignPlayerReferences(GameObject player)
    {   
        trackedStats = player.GetComponent<PlayerStats>();
        healthBar.Init(trackedStats.maxHP);
        hungerBar.Init(trackedStats.maxHunger);
        trackedStats.onHPChangeClient += OnHPChange;
        trackedStats.onHungerChangeClient += OnHungerChange;
        healthBar.SetAlpha(0f);
        hungerBar.SetAlpha(0f);
    }
    **/

    private void CheckBars()
    {
        for (int i = 0; i < activeBars.Count; i++)
        {
            if (!activeBars[i].statRevealing)
            {
                activeBars.RemoveAt(i);
                i--;
            }
        }
    }

    private void AdjustBarSpacing()
    {
        int i = 0;
        float step = Time.deltaTime * barSpaceSpeed;
        float middleOffset = GetTotalWidth() / 2f;
        foreach (var bar in activeBars)
        {
            RectTransform barTransform = (RectTransform)bar.transform;
            float newXPos = i * barSpacing - middleOffset;
            Vector2 targetLocation = new Vector2(newXPos, 0f);
            barTransform.anchoredPosition = Vector2.MoveTowards(barTransform.anchoredPosition, targetLocation, step);
            i++;
        }
    }

    private float GetTotalWidth()
    {
        if (activeBars.Count == 0) return 0f;
        return (activeBars.Count - 1) * barSpacing + ((RectTransform)activeBars[activeBars.Count - 1].transform).sizeDelta.x;
    }

    private void AddActiveBar(BarUI bar)
    {
        if (!activeBars.Contains(bar))
        {
            activeBars.Add(bar);
            RectTransform rt = bar.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-(rt.sizeDelta.x / 2f), 0f);
        }
    }

    private void OnHPChange(int value)
    {
        healthBar.SetValue(trackedStats.HP);
        healthBar.RevealStat();
        AddActiveBar(healthBar);
    }

    private void OnHungerChange(int value)
    {
        hungerBar.SetValue(trackedStats.hunger);
        hungerBar.RevealStat();
        AddActiveBar(hungerBar);
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
