using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Linq;
using Photon.Voice.Unity.Demos;
using UnityEngine.UI;
using Unity.VisualScripting;

public class TabSchedule : MonoBehaviour
{
    public Transform blockHolder;
    public GameObject blockPrefab;
    public TextMeshProUGUI dateText;
    public int readDay;
    public Color primaryColor;
    public Color secondaryColor;
    Photon.Realtime.Player selectedPlayer;
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
        hourHeight = blockHolder.GetComponent<RectTransform>().sizeDelta.y / (sm.immutableBlocks[sm.immutableBlocks.Count - 1].time - sm.immutableBlocks[0].time + 1);
    }

    private void OnDisable()
    {
        selectedPlayer = null;
    }

    // Shows the schedule a day in advance
    public void ScrollForward()
    {
        readDay++;
        if (selectedPlayer != null) DisplaySchedule(selectedPlayer);
    }

    // Shows the schedule a day before
    public void ScrollBackward()
    {
        readDay--;
        if (readDay < 0) readDay = 0;
        if (selectedPlayer != null) DisplaySchedule(selectedPlayer);
    }

    public void DisplaySchedule(Photon.Realtime.Player player)
    {
        selectedPlayer = player;
        List<ScheduleBlock> blocks = new List<ScheduleBlock>();
        float minRange = readDay * 24 - 1;
        float maxRange = readDay * 24 + 23;

        // Add immutable blocks
        foreach (ScheduleBlock block in sm.immutableBlocks)
        {
            ScheduleBlock nBlock = new ScheduleBlock(block.periodName, block.room, block.length, block.time + (gm.currentDay * 24));
            blocks.Add(block);
        }

        // Add mutable blocks from selected player
        foreach (ScheduleBlock block in sm.playerSchedules[player])
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
            blocks.Add(new ScheduleBlock("Free Time", "", blocksCheck[nextI].time - (blocksCheck[i].time + blocksCheck[i].length), blocksCheck[i].time + blocksCheck[i].length));
        }

        blocks = blocks.OrderBy(o => o.time).ToList();
        UpdateSchedule(blocks);

        // Change day text
        dateText.text = gm.GetDay(readDay);
    }

    // Sets the read day to the current day
    public void ResetReadDay(Photon.Realtime.Player player = null)
    {
        readDay = gm.currentDay;
    }

    public void DeselectSchedule(Photon.Realtime.Player player = null)
    {
        ClearSchedule();
        dateText.text = "...";
    }

    void UpdateSchedule(List<ScheduleBlock> blocks)
    {
        // Clears blocks
        ClearSchedule();

        int i = 0;
        // Instantiates new blocks with color
        foreach (ScheduleBlock block in blocks)
        {
            // Creates block
            GameObject newBlock = Instantiate(blockPrefab, blockHolder);
            RectTransform bt = newBlock.GetComponent<RectTransform>();

            bt.SetHeight(block.length * hourHeight);
            bt.GetChild(0).GetComponent<TextMeshProUGUI>().text = block.periodName;

            if (block.length < 1f)
            {
                Destroy(bt.GetChild(1).gameObject);
                Destroy(bt.GetChild(2).gameObject);
                continue;
            }

            bt.GetChild(1).GetComponent<TextMeshProUGUI>().text = block.room;
            bt.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{gm.PeriodToClockString(block.time)} - {gm.PeriodToClockString(block.time + block.length)}";

            Image nbI = newBlock.GetComponent<Image>();

            // Sets color
            if (i % 2 == 0)
            {
                nbI.color = primaryColor;
            } else
            {
                nbI.color = secondaryColor;
            }
            i++;

        }
    }

    void ClearSchedule()
    {
        foreach (Transform child in blockHolder)
        {
            Destroy(child.gameObject);
        }
    }
}
