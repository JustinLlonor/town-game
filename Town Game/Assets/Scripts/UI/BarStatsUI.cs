using Mono.Cecil.Cil;
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
    public Transform nodeHolder;
    public GameObject hotbarNodePrefab;
    public float barSpacing = 165f;
    public float barSpaceSpeed = 5f;
    private List<HotbarNode> activeNodes = new List<HotbarNode>();
    PlayerStats trackedStats;
    PlayerNodes trackedNodes;
    public FlowchartUI flowchartUI;
    float shMinHeight;
    bool init = false;
    float hotbarScale;
    bool fInit = false;

    private void Awake()
    {
        FindFirstObjectByType<PlayerManager>().onInstantiatePlayer += AssignPlayerReferences;
        //flowchartUI.onInfoSend += CheckShowNode;
    }

    private void LateUpdate()
    {
        SetHeight();
        CheckBars();
        AdjustBarSpacing();
    }

    void AssignPlayerReferences(GameObject player)
    {   
        trackedStats = player.GetComponent<PlayerStats>();
        trackedNodes = player.GetComponent<PlayerNodes>();
        trackedNodes.onNodeValueChange += ValueUpdate;
        //healthBar.Init(trackedStats.maxHP);
        //hungerBar.Init(trackedStats.maxHunger);
        //trackedStats.onHPChangeClient += OnHPChange;
        //trackedStats.onHungerChangeClient += OnHungerChange;
        //healthBar.SetAlpha(0f);
        //hungerBar.SetAlpha(0f);
    }

    private void CheckBars()
    {
        for (int i = 0; i < activeNodes.Count; i++)
        {
            if (!activeNodes[i].statRevealing)
            {
                Destroy(activeNodes[i].gameObject);
                activeNodes.RemoveAt(i);
                i--;
            }
        }
    }

    private void AdjustBarSpacing()
    {
        int i = 0;
        float step = Time.deltaTime * barSpaceSpeed;
        float middleOffset = GetTotalWidth() / 2f;
        foreach (var bar in activeNodes)
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
        if (activeNodes.Count == 0) return 0f;
        return (activeNodes.Count - 1) * barSpacing + ((RectTransform)activeNodes[activeNodes.Count - 1].transform).sizeDelta.x;
    }

    private void CheckShowNode(int nodeId)
    {
        int nodeIndex = GetNodeIndex(nodeId);
        if (nodeIndex > -1)
        {
            activeNodes[nodeIndex].ResetTime();
        }
        else
        {
            AddActiveNode(nodeId);
        }
    }

    private int GetNodeIndex(int nodeId)
    {
        for (int i = 0; i < activeNodes.Count; i++)
        {
            if (activeNodes[i].trackedNodeId == nodeId) return i;
        }
        return -1;
    }

    private void AddActiveNode(int nodeId)
    {
        GameObject nodeObject = Instantiate(hotbarNodePrefab, nodeHolder);
        HotbarNode node = nodeObject.GetComponent<HotbarNode>();
        node.Init(nodeId, trackedNodes);
        if (!activeNodes.Contains(node))
        {
            activeNodes.Add(node);
            RectTransform rt = nodeObject.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-(rt.sizeDelta.x / 2f), 0f);
        }
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

    private void ValueUpdate(Node node)
    {
        NodeInfo nodeInfo = trackedNodes.GetNodeInfo(node.infoIndex);
        if ((nodeInfo.criticalDisplayRange.x <= node.value) && (nodeInfo.criticalDisplayRange.y >= node.value))
        {
            CheckShowNode(node.id);
        }
    }
}
