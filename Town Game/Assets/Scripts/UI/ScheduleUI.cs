using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Photon.Pun.Demo.Procedural;

public class ScheduleUI : MonoBehaviour
{
    public ScheduleBlock testBlock;
    public int foresight = 3;
    public float hourLength = 50f;
    public float repositionSpeed = 3f;
    public GameObject scheduleBlockPrefab;
    public GameObject tearoutPrefab;
    public Transform blockHolder;
    public Transform tearoutHolder;
    public string emptyPeriod = "Free Time";
    [SerializeField] List<UIBlock> listedBlocks = new List<UIBlock>();
    GameObject tearout;
    GameManager gm;
    ScheduleManager sm;

    [System.Serializable]
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
        ((RectTransform)blockHolder).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, hourLength * (float)foresight);
    }

    private void OnEnable()
    {
        ReadSchedule();
        gm.OnDayStart += ReadSchedule;
        sm.OnUpdateSchedule += ReadSchedule;
        sm.OnBlockChange += UpdateTearout;
    }

    private void OnDisable()
    {
        gm.OnDayStart -= ReadSchedule;
        sm.OnUpdateSchedule -= ReadSchedule;
        sm.OnBlockChange -= UpdateTearout;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) AddScheduleBlock(new ScheduleBlock(testBlock.periodName, testBlock.room, testBlock.length, testBlock.time));
        ScrollSchedule();
    }

    void ScrollSchedule()
    {
        if (listedBlocks.Count == 0) return;
        if (BlockPassed(listedBlocks[0].block))
        {
            RemoveScheduleBlock(0);
        }
        float currentPeriod = gm.currentPeriod - (gm.currentDay * 24f);
        blockHolder.localPosition = new Vector2(0f, hourLength * currentPeriod);
    }

    /// <summary>
    /// Checks if the specified period has already passed in game time
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    bool BlockPassed(ScheduleBlock block)
    {
        return block.time + block.length < gm.currentPeriod;
    }

    void ReadSchedule()
    {
        ClearScheduleBlocks();

        List<ScheduleBlock> blocks = new List<ScheduleBlock>();
        float minRange = gm.currentDay * 24 - 1;
        float maxRange = gm.currentDay * 24 + 23;

        // Add immutable blocks
        foreach (ScheduleBlock block in sm.immutableBlocks)
        {
            if (BlockPassed(new ScheduleBlock(block.periodName, block.room, block.length, block.time + (gm.currentDay * 24)))) continue;
            ScheduleBlock nBlock = new ScheduleBlock(block.periodName, block.room, block.length, block.time + (gm.currentDay * 24));
            blocks.Add(nBlock);
        }

        // Add mutable blocks
        foreach (ScheduleBlock block in sm.schedule)
        {
            if (BlockPassed(block)) continue;
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
            if (BlockPassed(new ScheduleBlock(emptyPeriod, "", blocksCheck[nextI].time - (blocksCheck[i].time + blocksCheck[i].length), blocksCheck[i].time + blocksCheck[i].length))) continue; // ... fuck you
            blocks.Add(new ScheduleBlock(emptyPeriod, "", blocksCheck[nextI].time - (blocksCheck[i].time + blocksCheck[i].length), blocksCheck[i].time + blocksCheck[i].length));
        }

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
        float currentPosition = (block.time - (gm.currentDay * 24f)) * hourLength;
        nbt.localPosition = new Vector2(0f, -currentPosition);
        RectTransform rt = (RectTransform)nbt;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, block.length * hourLength);

        int setPos = 0;
        for (int i = 0; i < listedBlocks.Count; i++)
        {
            if (block.time > listedBlocks[i].block.time) setPos++;
        }
        listedBlocks.Insert(setPos, new UIBlock(block, nbt));

        // Sets text data on block
        nbt.GetChild(0).GetComponent<TextMeshProUGUI>().text = block.periodName;
        nbt.GetChild(1).GetComponent<TextMeshProUGUI>().text = block.room;
        // Time data
        string clockTimeStart = gm.PeriodToClockString(block.time);
        string clockTimeEnd = gm.PeriodToClockString(block.time + block.length);
        nbt.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{clockTimeStart} - {clockTimeEnd}";
    }

    void RemoveScheduleBlock(int index)
    {
        Destroy(listedBlocks[index].transform.gameObject);
        listedBlocks.RemoveAt(index);
    }

    void ClearScheduleBlocks()
    {
        listedBlocks.Clear();
        foreach (Transform child in blockHolder) Destroy(child.gameObject);
    }

    void UpdateTearout(ScheduleBlock from, ScheduleBlock to)
    {
        if (tearout != null) Destroy(tearout);
        if (to == null) return;
        GameObject newTearout = Instantiate(tearoutPrefab, tearoutHolder);
        Transform nbt = newTearout.transform;
        nbt.GetChild(0).GetComponent<TextMeshProUGUI>().text = to.periodName;
        nbt.GetChild(1).GetComponent<TextMeshProUGUI>().text = to.room;
        // Time data
        string clockTimeStart = gm.PeriodToClockString(to.time);
        string clockTimeEnd = gm.PeriodToClockString(to.time + to.length);
        nbt.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{clockTimeStart} - {clockTimeEnd}";
    }
}
