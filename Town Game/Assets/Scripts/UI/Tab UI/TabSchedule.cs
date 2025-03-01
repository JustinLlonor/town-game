using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using Fusion;

public class TabSchedule : MonoBehaviour
{
    public Transform blockHolder;
    public GameObject blockPrefab;
    public TextMeshProUGUI dateText;
    public int readDay;
    public Color primaryColor;
    public Color secondaryColor;
    PlayerRef selectedPlayer;
    ScheduleManager sm;
    GameManager gm;
    public float hourHeight = 0f;

    private void Awake()
    {
        sm = FindFirstObjectByType<ScheduleManager>();
        gm = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        List<ScheduleBlock> sortedBlocks = sm.dailyBlocks.OrderBy(o => o.time).ToList();
        ScheduleBlock lastBlock = sortedBlocks[sm.dailyBlocks.Count - 1];
        hourHeight = blockHolder.GetComponent<RectTransform>().sizeDelta.y / ((lastBlock.time + lastBlock.length) - (sortedBlocks[0].time));
    }

    private void OnDisable()
    {
        selectedPlayer = PlayerRef.None;
    }

    // Shows the schedule a day in advance
    public void ScrollForward()
    {
        readDay++;
        if (selectedPlayer != PlayerRef.None) DisplaySchedule(selectedPlayer);
    }

    // Shows the schedule a day before
    public void ScrollBackward()
    {
        if (readDay == 0) return;
        readDay--;
        if (selectedPlayer != PlayerRef.None) DisplaySchedule(selectedPlayer);
    }

    public void DisplaySchedule(PlayerRef player)
    {
        selectedPlayer = player;
        List<ScheduleBlock> blocks = new List<ScheduleBlock>();
        float minRange = readDay * 24 - 1;
        float maxRange = readDay * 24 + 23;

        // Add immutable blocks
        foreach (ScheduleBlock block in sm.dailyBlocks)
        {
            ScheduleBlock nBlock = new ScheduleBlock(block.periodName.ToString(), block.room.ToString(), block.length, block.time + (gm.currentDay * 24));
            blocks.Add(block);
        }

        // Add mutable blocks from selected player
        foreach (ScheduleBlock block in sm.proxySchedules[player])
        {
            if (block.time < minRange || block.time > maxRange) continue;
            blocks.Add(block);
        }

        /**
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
        **/

        blocks = blocks.OrderBy(o => o.time).ToList();
        UpdateSchedule(blocks);

        // Change day text
        dateText.text = gm.GetDay(readDay);
    }

    // Sets the read day to the current day
    public void ResetReadDay(PlayerRef player)
    {
        readDay = gm.currentDay;
    }

    public void DeselectSchedule(PlayerRef player)
    {
        ClearSchedule();
        selectedPlayer = PlayerRef.None;
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
            UIBlockPhys ubp = newBlock.GetComponent<UIBlockPhys>();

            bt.sizeDelta = new Vector3(bt.sizeDelta.x, (block.length * hourHeight));
            float roundedTime = block.time - 24f * gm.currentDay;
            float yPos = (roundedTime - 7f) * hourHeight;
            bt.localPosition = new Vector3(0f, -yPos);
            ubp.SetBlockColor(block.color);
            ubp.SetNameText(block.periodName);
            ubp.SetRoomText(block.room);
            //ubp.SetTimeText($"{gm.PeriodToClockString(block.time)} - {gm.PeriodToClockString(block.time + block.length)}");

            /**
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
            **/
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
