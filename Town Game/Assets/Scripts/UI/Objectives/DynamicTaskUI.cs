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
    [Header("Manager References")]
    public TaskCEventManager eventManager;
    public InputManager inputManager;
    public BranchManager branchManager;

    private string currentTask;
    private int currentSubtask;
    TaskHandler currentHandler;

    private void Start()
    {
        eventManager.onAssignTask += AddTask;
        eventManager.onUnassignTask += RemoveTask;
        eventManager.onCompleteTask += CompleteTask;
        inputManager.onScheduleSwap += CycleTask;
    }

    private void Update()
    {
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

    }
}
