using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class ScheduleUI : MonoBehaviour
{
    public ScheduleBlock testBlock;
    public float blockDistance = 50f;
    public GameObject scheduleBlockPrefab;
    List<UIBlock> listedBlocks = new List<UIBlock>();
    GameManager gm;
    ScheduleManager sm;

    public class UIBlock
    {
        public ScheduleBlock block;
        public Transform transform;

        public UIBlock(ScheduleBlock block, Transform transform)
        {
            this.block = block;
            this.transform = transform;
        }
    }

    private void Awake()
    {
        sm = FindObjectOfType<ScheduleManager>();
        gm = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        sm.OnUpdateSchedule += ReadSchedule;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) AddScheduleBlock(testBlock);
        if (Input.GetKeyDown(KeyCode.I)) RemoveScheduleBlock(testBlock);
    }

    void ReadSchedule()
    {
        // Remove excess blocks
        foreach (UIBlock uBlock in listedBlocks)
        {
            if (!sm.schedule.Any(n => n.Equals(uBlock.block)))
            {
                RemoveScheduleBlock(uBlock.block);
            }
        }

        // Add unadded blocks
        foreach (ScheduleBlock block in sm.schedule)
        {
            if (!listedBlocks.Any(n => n.block.Equals(block)))
            {
                AddScheduleBlock(block);
            }
        }
    }

    void AddScheduleBlock(ScheduleBlock block)
    {
        GameObject newBlock = Instantiate(scheduleBlockPrefab, transform);
        Transform nbt = newBlock.transform;
        listedBlocks.Add(new UIBlock(block, nbt));

        // Sets text data on block
        nbt.GetChild(0).GetComponent<TextMeshProUGUI>().text = block.periodName;
        nbt.GetChild(1).GetComponent<TextMeshProUGUI>().text = block.room;
        // Time data, also i know this is some spaghetti and can be simplified but give me a break smh
        Vector2Int clockTimeStart = gm.PeriodToClockTime(block.time);
        Vector2Int clockTimeEnd = gm.PeriodToClockTime(block.time + block.length);
        string startMins = clockTimeStart.y.ToString();
        if (startMins.Length == 1) startMins = "0" + startMins;
        string endMins = clockTimeEnd.y.ToString();
        if (endMins.Length == 1) endMins = "0" + endMins;
        string startMeridiem = "AM";
        if (clockTimeStart.x > 10 && clockTimeStart.x != 23) startMeridiem = "PM";
        string endMeridiem = "AM";
        if (clockTimeEnd.x > 10 && clockTimeEnd.x != 23) endMeridiem = "PM";
        clockTimeStart.x++;
        clockTimeEnd.x++;
        if (clockTimeStart.x > 12) clockTimeStart.x -= 12;
        if (clockTimeEnd.x > 12) clockTimeEnd.x -= 12;
        nbt.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{clockTimeStart.x}:{startMins} {startMeridiem} - {clockTimeEnd.x}:{endMins} {endMeridiem}";


    }

    void RemoveScheduleBlock(ScheduleBlock block)
    {
        for (int i = 0; i < listedBlocks.Count; i++)
        {
            UIBlock checkBlock = listedBlocks[i];
            if (checkBlock.block.Equals(block))
            {
                Destroy(checkBlock.transform.gameObject);
                listedBlocks.RemoveAt(i);
                return;
            }
        }

        Debug.LogWarning("Schedule block could not be found.");
    }

    // Sorts the schedule blocks in the list and re-orders them in game
    void SortScheduleBlocks()
    {

    }

    void ClearScheduleBlocks()
    {
        foreach (Transform child in transform)
        {
            Destroy(child);
        }
    }
}
