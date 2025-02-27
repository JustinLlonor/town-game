using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScheduleUI : MonoBehaviour
{
    public int foresight = 3;
    public float hourLength = 50f;
    public float repositionSpeed = 3f;
    public GameObject scheduleBlockPrefab;
    public GameObject tearoutPrefab;
    public GameObject bookmarkPrefab;
    public Transform blockHolder;
    public Transform tearoutHolder;
    public Transform bookmarkHolder;
    [Header("Block Settings")]
    public string emptyPeriod = "Free Time";
    public Color primaryColor;
    public Color secondaryColor;
    [Header("Bookmarks")]
    public float bookmarkOffset = 5f;
    public Color innoBookmark;
    public Color cultistBookmark;
    List<UIBlock> listedBlocks = new List<UIBlock>();
    GameObject tearout;
    GameManager gm;
    ScheduleManager sm;
    public float tearoutRemovalTime = -1;

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
        sm = FindFirstObjectByType<ScheduleManager>();
        gm = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        ((RectTransform)blockHolder).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, hourLength * (float)foresight);
    }

    private void OnEnable()
    {
        ReadSchedule();
        gm.OnChangeDay += ReadSchedule;
        sm.OnUpdateSchedule += ReadSchedule; // Make so that tearout is updated when schedule is updated
        sm.OnBlockChange += UpdateTearout;
    }

    private void OnDisable()
    {
        gm.OnChangeDay -= ReadSchedule;
        sm.OnUpdateSchedule -= ReadSchedule;
        sm.OnBlockChange -= UpdateTearout;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.O)) AddScheduleBlock(new ScheduleBlock(testBlock.periodName, testBlock.room, testBlock.length, testBlock.time));
        ScrollSchedule();
        CheckTearout();
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
        bookmarkHolder.localPosition = new Vector2(0f, hourLength * currentPeriod);
    }

    void CheckTearout()
    {
        if (tearoutRemovalTime == -1f) return;
        if (!(gm.currentPeriod >= tearoutRemovalTime)) return;
        DestroyTearout();
        tearoutRemovalTime = -1f;
    }

    void DestroyTearout()
    {
        if (tearout == null) return;
        StartCoroutine(RemoveTearout(tearout.transform));
        tearout = null;
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

    /// <summary>
    /// Updates the UI schedule to reflect the day
    /// </summary>
    void ReadSchedule()
    {
        ClearScheduleBlocks();

        // Create list of blocks to become schedule blocks
        List<ScheduleBlock> blocks = new List<ScheduleBlock>();
        float minRange = gm.currentDay * 24 - 1;
        float maxRange = gm.currentDay * 24 + 23;

        // Add immutable blocks
        foreach (ScheduleBlock block in sm.dailyBlocks)
        {
            if (BlockPassed(new ScheduleBlock(block.periodName.ToString(), block.room.ToString(), block.length, block.time + (gm.currentDay * 24)))) continue;
            ScheduleBlock nBlock = new ScheduleBlock(block.periodName.ToString(), block.room.ToString(), block.length, block.time + (gm.currentDay * 24));
            blocks.Add(nBlock);
        }

        // Adds all schedule blocks in the day
        foreach (ScheduleBlock block in sm.localSchedule)
        {
            if (BlockPassed(block)) continue;
            if (block.time < minRange || block.time > maxRange) continue; // If outside the time range, don't add
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
            ScheduleBlock emptySpace = new ScheduleBlock(emptyPeriod, "", blocksCheck[nextI].time - (blocksCheck[i].time + blocksCheck[i].length), blocksCheck[i].time + blocksCheck[i].length);
            // If we passed the empty space, then continue
            if (BlockPassed(emptySpace)) continue; 
            blocks.Add(emptySpace);
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
        nbt.GetChild(1).GetComponent<TextMeshProUGUI>().text = block.periodName.ToString();
        nbt.GetChild(2).GetComponent<TextMeshProUGUI>().text = block.room.ToString();
        // Time data
        string clockTimeStart = gm.PeriodToClockString(block.time);
        string clockTimeEnd = gm.PeriodToClockString(block.time + block.length);
        nbt.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"{clockTimeStart} - {clockTimeEnd}";
    }

    void RemoveScheduleBlock(int index)
    {
        Destroy(listedBlocks[index].transform.gameObject);
        listedBlocks.RemoveAt(index);
    }

    /// <summary>
    /// Clears UI listed blocks
    /// </summary>
    void ClearScheduleBlocks()
    {
        listedBlocks.Clear();
        foreach (Transform child in blockHolder) Destroy(child.gameObject);
    }

    void UpdateTearout(ScheduleBlock from, ScheduleBlock to)
    {
        if (tearout != null) DestroyTearout();
        string periodName;
        string room = "";
        float timeStart;
        float timeEnd;
        // Empty periods
        if (to.Equals(ScheduleBlock.None))
        {
            int afterIndex = sm.orderedBlocks.IndexOf(from) + 1;
            if (afterIndex > sm.orderedBlocks.Count - 1) return;
            timeStart = from.time + from.length;
            timeEnd = sm.orderedBlocks[afterIndex].time;
            periodName = emptyPeriod;
        }
        else
        {
            periodName = to.periodName.ToString();
            room = to.room.ToString();
            timeStart = to.time;
            timeEnd = to.time + to.length;
        }
        // Removal Time
        tearoutRemovalTime = timeEnd - 0.95f;
        // Text data
        GameObject newTearout = Instantiate(tearoutPrefab, tearoutHolder);
        Transform nbt = newTearout.transform;
        tearout = newTearout;
        nbt.GetChild(0).GetComponent<TextMeshProUGUI>().text = periodName;
        nbt.GetChild(1).GetComponent<TextMeshProUGUI>().text = room;
        // Time data
        string clockTimeStart = gm.PeriodToClockString(timeStart);
        string clockTimeEnd = gm.PeriodToClockString(timeEnd);
        nbt.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{clockTimeStart} - {clockTimeEnd}";
    }

    IEnumerator RemoveTearout(Transform t)
    {
        float time = 0f;
        float endX = t.localPosition.x + 365f;
        float ogX = t.localPosition.x;
        while (time < 1f)
        {
            yield return null;
            time += Time.deltaTime;
            float newX = Mathf.SmoothStep(ogX, endX, time);
            t.localPosition = new Vector2(newX, t.localPosition.y);
        }
        Destroy(t.gameObject);
    }

    /**
    void AddBookmarks()
    {
        ClearBookmarks();
        foreach (GlobalEvent ge in sm.globalEvents)
        {
            Color newColor = innoBookmark;
            if (ge.cultistEvent) newColor = cultistBookmark;

            GameObject newBookmark = Instantiate(bookmarkPrefab, bookmarkHolder);
            float currentPosition = -ge.time * hourLength;
            float offset = -bookmarkOffset;
            if (ge.cultistEvent) offset = bookmarkOffset;
            RectTransform rt = newBookmark.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, ge.length * hourLength);
            rt.localPosition = new Vector2(offset, currentPosition + hourLength * 3); // idk why its this number specifically
            newBookmark.GetComponent<RawImage>().color = newColor;
        }
    }
    **/

    void ClearBookmarks()
    {
        foreach (Transform child in bookmarkHolder)
        {
            Destroy(child.gameObject);
        }
    }
}
