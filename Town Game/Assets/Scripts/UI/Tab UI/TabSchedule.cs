using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Linq;
using Photon.Voice.Unity.Demos;
using UnityEngine.UI;

public class TabSchedule : MonoBehaviour
{
    public Transform blockHolder;
    public GameObject blockPrefab;
    public TextMeshProUGUI dateText;
    public int readDay;
    public Photon.Realtime.Player selectedPlayer;
    public Color primaryColor;
    public Color secondaryColor;
    ScheduleManager sm;
    GameManager gm;
    public float hourHeight = 0f;

    private void Awake()
    {
        sm = FindObjectOfType<ScheduleManager>();
        gm = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        Debug.Log(sm.immutableBlocks[sm.immutableBlocks.Count - 1].time - sm.immutableBlocks[0].time + 1);
        hourHeight = blockHolder.GetComponent<RectTransform>().sizeDelta.y / (sm.immutableBlocks[sm.immutableBlocks.Count - 1].time - sm.immutableBlocks[0].time + 1);
        ReadSchedule();
    }

    // Shows the schedule a day in advance
    public void ScrollForward()
    {

    }

    // Shows the schedule a day before
    public void ScrollBackward()
    {

    }

    public void ReadSchedule()
    {
        List<ScheduleBlock> blocks = new List<ScheduleBlock>();
        float minRange = gm.currentDay * 24 - 1;
        float maxRange = gm.currentDay * 24 + 23;

        // Add immutable blocks
        foreach (ScheduleBlock block in sm.immutableBlocks)
        {
            ScheduleBlock nBlock = new ScheduleBlock(block.periodName, block.room, block.length, block.time + (gm.currentDay * 24));
            blocks.Add(block);
        }

        // Add mutable blocks
        foreach (ScheduleBlock block in sm.schedule)
        {
            if (block.time < minRange || block.time > maxRange) continue;
            blocks.Add(block);
        }

        // Sort blocks
        blocks = blocks.OrderBy(o => o.time).ToList();

        // Add empty spaces
        List<ScheduleBlock> blocksCheck = new List<ScheduleBlock>(blocks);
        for (int i = 0; i < blocksCheck.Count; i++)
        {
            int nextI = i + 1;
            if (nextI >= blocksCheck.Count) break;
            if (blocksCheck[i].time + blocksCheck[i].length == blocksCheck[nextI].time) continue; // Continue if there is no space in between blocks
            blocks.Add(new ScheduleBlock("Camp Maintanence", "Assigned Rooms", blocksCheck[nextI].time - (blocksCheck[i].time + blocksCheck[i].length), blocksCheck[i].time + blocksCheck[i].length));
        }

        blocks = blocks.OrderBy(o => o.time).ToList();
        UpdateSchedule(blocks);
    }

    void UpdateSchedule(List<ScheduleBlock> blocks)
    {
        // Clears blocks
        foreach (Transform child in blockHolder)
        {
            Destroy(child.gameObject);
        }
        
        // Instantiates new blocks
        foreach (ScheduleBlock block in blocks)
        {
            // Creates block
            GameObject newBlock = Instantiate(blockPrefab, blockHolder);
            RectTransform bt = newBlock.GetComponent<RectTransform>();

            bt.SetHeight(block.length * hourHeight);
            Debug.Log(block.length);
            bt.GetChild(0).GetComponent<TextMeshProUGUI>().text = block.periodName;

            if (block.length < 1f)
            {
                Destroy(bt.GetChild(1).gameObject);
                Destroy(bt.GetChild(2).gameObject);
                continue;
            }

            bt.GetChild(1).GetComponent<TextMeshProUGUI>().text = block.room;
            bt.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{gm.PeriodToClockString(block.time)} - {gm.PeriodToClockString(block.time + block.length)}";
        }

        ColorSchedule();
    }

    void ColorSchedule()
    {
        foreach (Transform child in blockHolder)
        {
            if (child.GetSiblingIndex() % 2 == 0)
            {
                child.GetComponent<Image>().color = secondaryColor;
                continue;
            }
            child.GetComponent<Image>().color = primaryColor;
        }
    }
}
