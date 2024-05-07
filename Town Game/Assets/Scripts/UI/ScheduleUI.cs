using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ScheduleUI : MonoBehaviour
{
    public int foresight = 3;
    public float blockDistance = 50f;
    public float repositionSpeed = 3f;
    public GameObject scheduleBlockPrefab;
    public Transform blockHolder;
    public Transform minimapTransform;
    public string emptyPeriod = "Camp Maintanence";
    float minimapY;
    List<UIBlock> listedBlocks = new List<UIBlock>();
    List<IEnumerator> sortRoutines = new List<IEnumerator>();
    IEnumerator mapRoutine = null;
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
        minimapY = minimapTransform.localPosition.y;
        sm.OnUpdateSchedule += ReadSchedule;
    }

    private void Start()
    {
        ((RectTransform)blockHolder).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, blockDistance * (float)foresight);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.O)) AddScheduleBlock(new ScheduleBlock(testBlock.periodName, testBlock.room, testBlock.length, testBlock.time));
        //if (Input.GetKeyDown(KeyCode.I)) RemoveScheduleBlock(testBlock);
    }

    void ReadSchedule()
    {
        List<ScheduleBlock> blocks = new List<ScheduleBlock>();

        // Add immutable blocks
        foreach (ScheduleBlock block in sm.immutableBlocks)
        {
            blocks.Add(block);
        }

        // Add mutable blocks
        foreach (ScheduleBlock block in sm.schedule)
        {
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
            blocks.Add(new ScheduleBlock(emptyPeriod, "Assigned Rooms", blocksCheck[nextI].time - (blocksCheck[i].time + blocksCheck[i].length), blocksCheck[i].time + blocksCheck[i].length));
        }

        Debug.Log(blocks.Count);
        GroupAddScheduleBlocks(blocks);
    }

    void GroupAddScheduleBlocks(List<ScheduleBlock> blocks)
    {
        blocks = blocks.OrderBy(o => o.time).ToList();

        foreach (ScheduleBlock block in blocks)
        {
            AddScheduleBlock(block);
        }
    }

    void AddScheduleBlock(ScheduleBlock block)
    {
        GameObject newBlock = Instantiate(scheduleBlockPrefab, blockHolder);
        Transform nbt = newBlock.transform;
        nbt.localPosition = new Vector2(0f, blockDistance);

        int setPos = 0;
        for (int i = 0; i < listedBlocks.Count; i++)
        {
            if (block.time > listedBlocks[i].block.time) setPos++;
        }
        listedBlocks.Insert(setPos, new UIBlock(block, nbt));
        if (setPos > foresight-1) nbt.localPosition = new Vector2(0f, -blockDistance * foresight);

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

        SortScheduleBlocks();
    }

    void RemoveScheduleBlock(ScheduleBlock block)
    {
        for (int i = 0; i < listedBlocks.Count; i++)
        {
            UIBlock checkBlock = listedBlocks[i];
            if (checkBlock.block.Equals(block))
            {
                StartCoroutine(RemoveBlock(checkBlock.transform));
                listedBlocks.RemoveAt(i);
                SortScheduleBlocks();

                return;
            }
        }

        Debug.LogWarning("Schedule block could not be found.");
    }

    // Sorts the schedule blocks in the list and re-orders them in game
    void SortScheduleBlocks()
    {
        foreach (IEnumerator routine in sortRoutines) StopCoroutine(routine);
        sortRoutines.Clear();
        for (int i = 0; i < listedBlocks.Count; i++)
        {
            IEnumerator newRoutine = SetBlockPosition(listedBlocks[i].transform, i);
            StartCoroutine(newRoutine);
            sortRoutines.Add(newRoutine);
        }
        if (mapRoutine != null) StopCoroutine(mapRoutine);
        mapRoutine = SetMinimapPosition();
        StartCoroutine(mapRoutine);
    }

    void ClearScheduleBlocks()
    {
        foreach (Transform child in transform)
        {
            Destroy(child);
        }
    }

    IEnumerator SetBlockPosition(Transform blockTransform, int index)
    {
        float desiredY = (float)index * -blockDistance;
        float time = 0f;
        float initialY = blockTransform.localPosition.y;
        while (blockTransform.localPosition.y != desiredY)
        {
            time += Time.deltaTime * repositionSpeed;
            float currentY = Mathf.SmoothStep(initialY, desiredY, time);
            blockTransform.localPosition = new Vector3(0f, currentY);
            yield return null;
        }
    }

    IEnumerator RemoveBlock(Transform blockTransform)
    {
        float time = 0f;
        float initialY = blockTransform.localPosition.y;
        while (blockTransform.localPosition.y != blockDistance)
        {
            time += Time.deltaTime * repositionSpeed;
            float currentY = Mathf.SmoothStep(initialY, blockDistance, time);
            blockTransform.localPosition = new Vector3(0f, currentY);
            yield return null;
        }
        Destroy(blockTransform.gameObject);
    }

    IEnumerator SetMinimapPosition()
    {
        float desiredY = minimapY + (Mathf.Clamp((float)listedBlocks.Count-1, 0, foresight-1) * -blockDistance);
        float originalY = minimapTransform.localPosition.y;
        float time = 0f;
        while (minimapTransform.localPosition.y != desiredY)
        {
            time += Time.deltaTime * repositionSpeed;
            float newY = Mathf.SmoothStep(originalY, desiredY, time);
            minimapTransform.localPosition = new Vector3(minimapTransform.localPosition.x, newY);
            yield return null;
        }

        yield return null;
    }
}
