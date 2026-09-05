using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DynamicTaskUI : MonoBehaviour
{
    [Header("Settings")]
    // How long task results will be shown after a task is completed
    public float completedViewDuration = 3f;
    [Header("Tracked fields")]
    public List<string> assignedTasks;
    public List<CompletionInfo> completedTasks;
    public int viewedTask;
    [Header("UI References")]
    public TextMeshProUGUI taskText;
    public TextMeshProUGUI subtaskText;
    public TextMeshProUGUI deadlineText;
    public RectTransform subtaskRT;
    public GameObject iconHolder;
    public RawImage iconImage;
    public TextMeshProUGUI moneyText;
    public GameObject nextHolder;
    public KeyUI keyUI;
    public TextMeshProUGUI nextText;
    public GameObject panelObject;
    public GameObject completePanel;
    public TextMeshProUGUI completedText;
    public TextMeshProUGUI moneyCText;
    public TextMeshProUGUI performanceCText;
    [Header("Manager References")]
    public TaskCEventManager eventManager;
    public InputManager inputManager;
    public BranchManager branchManager;
    public GameManager gameManager;

    private string currentTask;
    private int currentSubtask;
    TaskHandler currentHandler;
    private float completeTimer = 0;

    private void Start()
    {
        eventManager.onAssignTask += AddTask;
        eventManager.onUnassignTask += RemoveTask;
        eventManager.onCompleteTask += CompleteTask;
        inputManager.onScheduleSwap += CycleTask;
        SetTaskEmpty();
    }

    private void Update()
    {
        CompleteTimer();
        // Iterate over every task, get info
        if (assignedTasks.Count == 0)
        {
            if (currentTask != null)
            {
                // reset task data and return
                currentTask = null;
                currentSubtask = -1;
                SetTaskEmpty();
            }
            return;
        }
        panelObject.SetActive(true);
        // Activate task change event
        if (currentTask != assignedTasks[viewedTask])
        {
            currentTask = assignedTasks[viewedTask];
            ChangeTask();
        }

        CheckSubtaskStage();
    }

    /// <summary>
    /// Called when the task has been changed
    /// </summary>
    private void ChangeTask()
    {
        int branch = branchManager.GetBranch(branchManager.Runner.LocalPlayer);
        if (branch == -1)
        {
            Debug.LogError("Task is assigned but player is not assigned to a branch!");
            assignedTasks.Clear();
            return;
        }
        // Update current handler
        currentHandler = branchManager.branches[branch].branchHandler;

        // Get task info
        DynamicTask taskData = currentHandler.GetTask(assignedTasks[viewedTask]);
        // Change text info
        taskText.text = taskData.displayName;
        moneyText.text = "+" + currentHandler.GetReward(assignedTasks[viewedTask]);
        if (currentHandler.deadlines.ContainsKey(assignedTasks[viewedTask]))
        {
            deadlineText.text = "Due " +
                        gameManager.PeriodToClockString(currentHandler.deadlines.Get(assignedTasks[viewedTask]));
        }
        else
        {
            deadlineText.text = "";
        }
    }

    private void CheckSubtaskStage()
    {
        if (assignedTasks.Count == 0) return;
        int stage = currentHandler.GetTaskStage(assignedTasks[viewedTask]);
        if (currentSubtask == stage) return;
        currentSubtask = stage;
        UpdateSubtaskInfo();
    }

    private void UpdateSubtaskInfo()
    {
        Subtask subtask = currentHandler.GetTask(assignedTasks[viewedTask]).subtasks[currentSubtask];
        subtaskText.text = subtask.displayName;
        iconImage.texture = subtask.icon;
    }

    /// <summary>
    /// Called when there is no current task
    /// </summary>
    private void SetTaskEmpty()
    {
        panelObject.SetActive(false);
        nextHolder.SetActive(false);
    }

    private void CycleTask()
    {
        viewedTask++;
        if (viewedTask >= assignedTasks.Count) viewedTask = 0;
    }

    private void AddTask(string task)
    {
        if (assignedTasks.Contains(task)) return;
        assignedTasks.Add(task);
        if (assignedTasks.Count >= 2)
        {
            nextHolder.SetActive(true);
        }
        UpdateNext();
    }

    private void RemoveTask(string task)
    {
        if (!assignedTasks.Contains(task)) return;
        int taskIndex = assignedTasks.IndexOf(task);
        assignedTasks.Remove(task);
        if (taskIndex <= viewedTask && viewedTask > 0) viewedTask--;
        if (assignedTasks.Count <= 1)
        {
            nextHolder.SetActive(false);
        }
        UpdateNext();
    }

    private void UpdateNext()
    {
        nextText.text = "Next task (" + viewedTask + "/" + assignedTasks.Count + ")";
    }

    private void CompleteTask(CompletionInfo info)
    {
        completedTasks.Add(info);
        if (completedTasks.Count == 1)
        {
            DisplayComplete();
        }
    }

    private void DisplayComplete()
    {
        if (completedTasks.Count == 0)
        {
            completePanel.SetActive(false);
            return;
        }
        completePanel.SetActive(true);
        // reset duration timer
        completeTimer = completedViewDuration;
        // Set to currently viewed info
        CompletionInfo info = completedTasks[0];
        DynamicTask taskInfo = currentHandler.GetTask((string)info.id);
        completedText.text = "Task completed: " + taskInfo.displayName;
        int punishPercent = Mathf.RoundToInt(info.punishmentPercentage * 100f);
        string moneyText = "+" + info.moneyChange + "$";
        if (punishPercent > 0) moneyText = moneyText + " (-" + punishPercent + "%)";
        moneyCText.text = moneyText;
        performanceCText.text = info.performanceChange + " perf.";
    }

    private void CompleteTimer()
    {
        if (completeTimer <= 0) return;
        completeTimer -= Time.deltaTime;
        if (completeTimer < 0)
        {
            completeTimer = 0;
            completedTasks.RemoveAt(0);
            DisplayComplete();
        }
    }
}
