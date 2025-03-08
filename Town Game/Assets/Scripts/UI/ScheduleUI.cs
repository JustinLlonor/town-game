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
    Dictionary<ScheduleBlock, List<UITask>> tearoutTasks = new Dictionary<ScheduleBlock, List<UITask>>();
    List<UITask> uiTasks = new List<UITask>(); // All the tasks that are displayed on a tearout currently, to be compared when there are changes
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
    bool taskRevealStarted = false;
    bool canStartReveal = false;

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

    class UITask
    {
        public string name;
        public bool completed;

        public UITask(string name, bool completed)
        {
            this.name = name;
            this.completed = completed;
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

    private void Update()
    {
        CheckTaskRevealStart();
    }

    // Replaces period task info with this
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendTearoutInfo([RpcTarget] PlayerRef player, string periodName, string room, float time, float length, string[] tasks, bool[] completed)
    {
        List<UITask> receivedTasks = new List<UITask>();
        // If the tearout tasks doesn't contain the key
        for (int i = 0; i < tasks.Length; i++)
        {
            receivedTasks.Add(new UITask(tasks[i], completed[i]));
        }

        ScheduleBlock infoBlock = new ScheduleBlock(periodName, room, length, time);
        // If we haven't added this yet
        ScheduleBlock tearoutKey = infoBlock.GetEquivalentBlockInSchedule(tearoutTasks.Keys.ToList());
        if (!tearoutTasks.ContainsKey(tearoutKey))
        {
            tearoutTasks.Add(infoBlock, receivedTasks);
            RenderCurrentTasks(false); // New thing created, dont render differences
        }
        else
        {
            tearoutTasks[tearoutKey] = receivedTasks;
            Debug.Log("Rendering true");
            RenderCurrentTasks(true); // Modified current thing, render the differences
        }
    }

    // Removes the tearout info of the specified tearout
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RemoveTearoutInfo([RpcTarget] PlayerRef player, string periodName, string room, float time, float length)
    {

    }

    // Sends the specified subtext
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendSubtext([RpcTarget] PlayerRef player, string periodName, string room, float time, float length, string subtext)
    {

    }

    private void RenderCurrentTasks(bool renderDifferences = false)
    {
        ScheduleBlock selectedTearout = previousBuffer;
        if (selectedTearout.GetEquivalentBlockInSchedule(tearoutTasks.Keys.ToList()) == null)
        {
            ClearUITasks();
            return;
        } // If the selected tearout has no tasks
        if (renderDifferences)
        {
            RenderTearoutDifferences(selectedTearout);
            return;
        }
        ClearUITasks();
        TearoutPhys tPhys = currentTearout.GetComponent<TearoutPhys>();
        if (tPhys == null) return;
        ScheduleBlock tearoutKey = selectedTearout.GetEquivalentBlockInSchedule(tearoutTasks.Keys.ToList());
        if (!tearoutTasks.ContainsKey(tearoutKey) || tearoutTasks[tearoutKey] == null) return;
        foreach (UITask task in tearoutTasks[tearoutKey])
        {
            AddUITask(task, tPhys);
        }
    }

    private void RenderTearoutDifferences(ScheduleBlock selectedTearout)
    {
        List<UITask> diffList = new List<UITask>();
        ScheduleBlock tearoutKey = selectedTearout.GetEquivalentBlockInSchedule(tearoutTasks.Keys.ToList());
        if (tearoutKey == null) return;
        if (tearoutTasks[tearoutKey] == null) return;
        diffList = tearoutTasks[tearoutKey];
        Debug.Log("Finding tearout differences");
        Debug.Log(diffList.Count);
        if (diffList.Count != uiTasks.Count || diffList.Count == 0)
        {
            //RenderCurrentTasks(false);
            return; // Render current tasks without finding differences
        }
        Debug.Log("Doing");
        for (int i = 0; i < uiTasks.Count; i++) // Find differences between uiTasks and diffList
        {
            Debug.Log("Before name");
            if (uiTasks[i].name != diffList[i].name) return;
            Debug.Log(uiTasks[i].completed);
            Debug.Log(diffList[i].completed);
            if (uiTasks[i].completed != diffList[i].completed)
            {
                Debug.Log("Found");
                uiTasks[i].completed = diffList[i].completed;
                SetUITaskCompleted(i, diffList[i].completed);
            }
        }
    }

    private void AddUITask(UITask task, TearoutPhys tPhys)
    {
        uiTasks.Add(task);
        Debug.Log("Adding new uitask");
        tPhys.AddUITask(task.name, task.completed);
    }

    private void SetUITaskCompleted(int blockIndex, bool completed)
    {
        if (currentTearout != null) currentTearout.GetComponent<TearoutPhys>().SetUITaskCompleted(blockIndex, completed);
    }

    private void ClearUITasks()
    {
        uiTasks.Clear();
        if (currentTearout != null) currentTearout.GetComponent<TearoutPhys>().ClearUITasks();
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
        RenderCurrentTasks();
    }

    void UpdateTearoutUI(bool isSwapping = false)
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

        canStartReveal = false;
        firstFrame = true;
        if (!currentBlock.Equals(ScheduleBlock.None))
        {
            GameObject newTearout = Instantiate(tearoutPrefab, tearoutHolder);
            UIBlockPhys pub = newTearout.GetComponent<UIBlockPhys>();
            SetUIBlockProperties(pub, currentBlock);
            if (isSwapping)
            {
                pub.PlayAnimation("TearoutSwap");
                canStartReveal = true;
                taskRevealStarted = false;
                ResetMinimapPosition();
            } // Animation for swapping tearout

            if (currentTearout != null) Destroy(currentTearout);
            currentTearout = newTearout;

            if (previousBuffer.Equals(ScheduleBlock.None))
            {
                RectTransform rt = (RectTransform)newTearout.transform;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 0f);
                StartCoroutine(StartTearoutHeightAnimation(newTearout, 0f, tearoutHeight, false, true));
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

    IEnumerator StartTearoutHeightAnimation(GameObject tearout, float startHeight, float endHeight, bool destroyAfterFinished = false, bool setCanStartReveal = false)
    {
        RectTransform rt = (RectTransform)tearout.transform;
        float height = startHeight;
        float progress = 0f;
        while (progress < 1f)
        {
            yield return null;
            if (rt == null) yield break;
            progress += Time.deltaTime * tearoutAnimationSpeed;
            height = Mathf.SmoothStep(startHeight, endHeight, progress);
            rt.sizeDelta = new Vector3(rt.sizeDelta.x, height);
        }
        if (rt != null) rt.sizeDelta = new Vector3(rt.sizeDelta.x, endHeight);

        if (destroyAfterFinished) Destroy(tearout);
        if (setCanStartReveal) canStartReveal = true;
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

    private void CheckTaskRevealStart()
    {
        // Starts the reveal the first available frame canStartReveal is true, and if reveal hasn't started
        if (!canStartReveal)
        {
            taskRevealStarted = false;
            return;
        }
        if (taskRevealStarted) return;

        if (currentTearout == null) return;
        TearoutPhys tp = currentTearout.GetComponent<TearoutPhys>();
        if (tp == null) return;

        float newHeight = tp.GetSubtextHeight();
        if (newHeight != 0f) newHeight += tp.padding;
        else return;
        taskRevealStarted = true;
        StartCoroutine(StartTearoutHeightAnimation(tp.gameObject, tearoutHeight, newHeight + tearoutHeight));
        StartMinimapAnimation(MinimapAnimation(tearoutHeight, newHeight + tearoutHeight + 4f));
    }

    private void ResetMinimapPosition()
    {
        if (minimapAnimation != null) StopCoroutine(minimapAnimation);
        minimapAnimation = null;
        minimapHolder.localPosition = new Vector3(minimapHolder.localPosition.x, originalMinimapY - tearoutHeight);
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
}
