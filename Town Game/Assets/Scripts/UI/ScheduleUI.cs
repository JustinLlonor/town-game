using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Fusion;

public class ScheduleUI : NetworkBehaviour
{
    public int foresight = 3;
    public float hourLength = 50f;
    public float repositionSpeed = 3f;
    public float tearoutHeight = 53f;
    public GameObject scheduleBlockPrefab;
    public GameObject tearoutPrefab;
    public GameObject bookmarkPrefab;
    public Transform blockHolder;
    public Transform bookmarkHolder;
    public Transform tearoutHolder;
    public Transform minimapHolder;
    public float tearoutAnimationSpeed = .5f;
    public InputActionReference tearoutSwap;
    List<ScheduleBlock> tearoutBuffer = new List<ScheduleBlock>();
    List<SubtextInfo> clientSubtextBuffer = new List<SubtextInfo>();
    GameObject currentTearout;
    ScheduleBlock previousBuffer = ScheduleBlock.None;
    IEnumerator minimapAnimation = null;
    [Header("Block Settings")]
    public string emptyPeriod = "Free Time";
    public Color primaryColor;
    public Color secondaryColor;
    List<UIBlock> listedBlocks = new List<UIBlock>();
    GameObject tearout;
    GameManager gm;
    ScheduleManager sm;
    public float tearoutRemovalTime = -1;
    float originalMinimapY = 0f;
    bool firstFrame = true;
    int previousKeyText = 0;

    [System.Serializable]
    class UIBlock
    {
        public ScheduleBlock block;
        public Transform transform;

        public UIBlock(ScheduleBlock block, Transform transform)
        {
            this.block = block;
            this.transform = transform;
        }
    }

    struct SubtextInfo
    {
        public ScheduleBlock trackedBlock;
        public string subText;
        public List<Task> tasks;

        public SubtextInfo(ScheduleBlock trackedBlock, string subText, List<Task> tasks)
        {
            this.trackedBlock = trackedBlock;
            this.subText = subText;
            this.tasks = tasks;
        }
    }

    private void Awake()
    {
        sm = FindFirstObjectByType<ScheduleManager>();
        gm = FindFirstObjectByType<GameManager>();
        sm.OnBlockStart += AddTearout;
        sm.OnBlockEnd += RemoveTearout;
        FindFirstObjectByType<InputManager>().onScheduleSwap += OnTearoutSwap;
        originalMinimapY = minimapHolder.localPosition.y;
    }

    private void Start()
    {
        ((RectTransform)blockHolder).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, hourLength * (float)foresight);
    }

    // Replaces period task info with this
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendTearoutInfo([RpcTarget] PlayerRef player, string periodName, string room, float time, float length, string[] tasks, bool[] completed)
    {

    }

    // Removes the tearout info of the specified tearout
    public void RPC_RemoveTearoutInfo([RpcTarget] PlayerRef player, string periodName, string room, float time, float length)
    {

    }

    // Sends the specified subtext
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendSubtext([RpcTarget] PlayerRef player, string periodName, string room, float time, float length, string subtext)
    {

    }

    void AddTearout(ScheduleBlock block)
    {
        tearoutBuffer.Add(block);
        UpdateTearoutUI();
    }

    void RemoveTearout(ScheduleBlock block)
    {
        tearoutBuffer.Remove(block);
        UpdateTearoutUI();
    }

    void OnTearoutSwap()
    {
        if (tearoutBuffer.Count <= 1) return;

        tearoutBuffer.Add(tearoutBuffer[0]);
        tearoutBuffer.RemoveAt(0);
        firstFrame = true;
        UpdateTearoutUI(true);
    }

    void UpdateTearoutUI(bool swapAnimation = false)
    {
        ScheduleBlock currentBlock = ScheduleBlock.None;
        if (tearoutBuffer.Count > 0) currentBlock = tearoutBuffer[0];

        if (previousBuffer.Equals(currentBlock)) { 
            if (firstFrame || previousKeyText != tearoutBuffer.Count)
            {
                firstFrame = false;
                UpdateTearoutBuffer();
            }
            return; 
        }

        firstFrame = true;
        if (!currentBlock.Equals(ScheduleBlock.None))
        {
            GameObject newTearout = Instantiate(tearoutPrefab, tearoutHolder);
            UIBlockPhys pub = newTearout.GetComponent<UIBlockPhys>();
            SetUIBlockProperties(pub, currentBlock);
            if (swapAnimation) pub.PlayAnimation("TearoutSwap");

            if (currentTearout != null) Destroy(currentTearout);
            currentTearout = newTearout;

            if (previousBuffer.Equals(ScheduleBlock.None))
            {
                RectTransform rt = (RectTransform)newTearout.transform;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 0f);
                StartCoroutine(StartTearoutHeightAnimation(newTearout, 0f, tearoutHeight));
                StartMinimapAnimation(MinimapAnimation(0f, tearoutHeight));
            }
            else
            {
                RectTransform rt = (RectTransform)newTearout.transform;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, tearoutHeight);
            }
            previousBuffer = currentBlock;
            // none -> something, play the animation
            UpdateTearoutBuffer();
        }
        else
        {
            previousBuffer = currentBlock;
            // previous is not current block, and current block is none, or previous was something and current is nothing
            if (currentTearout != null)
            {
                StartCoroutine(StartTearoutHeightAnimation(currentTearout, tearoutHeight, 0f, true)); // Start animation to destroy this
                StartMinimapAnimation(MinimapAnimation(tearoutHeight, 0f));
            }
        }
    }

    void UpdateTearoutBuffer()
    {
        previousKeyText = tearoutBuffer.Count;
        if (currentTearout == null) return;
        // Tearout overlap
        UIBlockPhys ubp = currentTearout.GetComponent<UIBlockPhys>();
        if (tearoutBuffer.Count > 1)
        {
            ubp.SetKeyVisibility(true);
            ubp.SetOverlap("(" + tearoutBuffer.Count + ")");
            string interactText = InputControlPath.ToHumanReadableString(
                tearoutSwap.action.bindings[0].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
            ubp.SetKeyText(interactText);
        }
        else
        {
            ubp.SetKeyVisibility(false);
        }
    }

    void StartMinimapAnimation(IEnumerator newAnimation)
    {
        if (minimapAnimation != null) StopCoroutine(minimapAnimation);
        minimapAnimation = newAnimation;
        StartCoroutine(minimapAnimation);
    }

    void SetUIBlockProperties(UIBlockPhys pub, ScheduleBlock block)
    {
        pub.SetNameText(block.periodName);
        pub.SetRoomText(block.room);
        string clockTimeStart = gm.PeriodToClockString(block.time);
        string clockTimeEnd = gm.PeriodToClockString(block.time + block.length);
        pub.SetTimeText($"{clockTimeStart} - {clockTimeEnd}");
        pub.SetBlockColor(block.color);
    }

    IEnumerator StartTearoutHeightAnimation(GameObject tearout, float startHeight, float endHeight, bool destroyAfterFinished = false)
    {
        RectTransform rt = (RectTransform)tearout.transform;
        float height = startHeight;
        float progress = 0f;
        while (progress < 1f)
        {
            yield return null;
            progress += Time.deltaTime * tearoutAnimationSpeed;
            height = Mathf.SmoothStep(startHeight, endHeight, progress);
            rt.sizeDelta = new Vector3(rt.sizeDelta.x, height);
        }
        rt.sizeDelta = new Vector3(rt.sizeDelta.x, endHeight);

        if (destroyAfterFinished) Destroy(tearout);
    }

    // Minimap animation done separately so it can cancel
    IEnumerator MinimapAnimation(float startHeight, float endHeight)
    {
        float height = startHeight;
        float progress = 0f;
        while (progress < 1f)
        {
            yield return null;
            progress += Time.deltaTime * tearoutAnimationSpeed;
            height = Mathf.SmoothStep(startHeight, endHeight, progress);
            minimapHolder.localPosition = new Vector3(minimapHolder.localPosition.x, originalMinimapY - height);
        }
        minimapHolder.localPosition = new Vector3(minimapHolder.localPosition.x, originalMinimapY - endHeight);

    }

    bool BlockPassed(ScheduleBlock block)
    {
        return block.time + block.length < gm.currentPeriod;
    }

    // Deprecated code
    #region
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
    #endregion

    /// <summary>
    /// Checks if the specified period has already passed in game time
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>

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

        /**
        // Add empty spaces (deprecated)
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
        **/

        GroupAddScheduleBlocks(blocks);
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

        // Sets color data (VERY SPAGHETTI, CHANGE LATER)
        if (!block.color.Equals(new Color()))
        {
            nbt.GetChild(0).GetChild(0).GetComponent<Image>().color = block.color;
            float h, s, v;
            Color.RGBToHSV(block.color, out h, out s, out v);
            v = Mathf.Clamp01(v - 0.45f);
            Color stripeColor = Color.HSVToRGB(h, s, v);
            nbt.GetChild(0).GetChild(0).GetChild(0).GetComponent<RawImage>().color = stripeColor;
        }
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
}
