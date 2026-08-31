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
    public RectTransform subtaskRT;
    public GameObject iconHolder;
    public RawImage iconImage;
    public TextMeshProUGUI moneyText;
    public GameObject nextHolder;
    public KeyUI keyUI;
    public TextMeshProUGUI nextText;
    [Header("Manager References")]
    public TaskCEventManager eventManager;
    public InputManager inputManager;

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
    }

    private void CycleTask()
    {

    }

    private void AddTask(string task)
    {

    }

    private void RemoveTask(string task)
    {

    }

    private void CompleteTask(CompletionInfo info)
    {

    }
}
