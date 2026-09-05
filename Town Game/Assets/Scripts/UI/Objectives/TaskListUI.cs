using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskListUI : MonoBehaviour
{
    public float minHeight;
    public float cancelDuration = 3f;    
    public GameObject taskPrefab;
    public GameObject taskResolutionPrefab;
    public Transform taskHolder;
    public List<UITask> uiTasks = new List<UITask>();
    private List<JobHandler> currentJobHandlers = new List<JobHandler>();
    private Dictionary<double, GameObject> cancellationTimers = new Dictionary<double, GameObject>();

    GameManager gameManager;

    [System.Serializable]
    public class UITask
    {
        public int taskId;
        public float deadline;
        public JobHandler handler;
        public GameObject uiObject;

        public UITask(int taskId, JobHandler handler, GameObject uiObject, float deadline)
        {
            this.taskId = taskId;
            this.handler = handler;
            this.uiObject = uiObject;
            this.deadline = deadline;
        }
    }

    private void Start()
    {
        PositionManager.i.onJobAdd += JobAdd;
        PositionManager.i.onJobRemove += JobRemove;
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void Update()
    {
        List<double> finishedCancellations = new List<double>();
        foreach (var kvp in cancellationTimers)
        { 
            if (Time.realtimeSinceStartupAsDouble > kvp.Key)
            {
                finishedCancellations.Add(kvp.Key);
            }
        }
        foreach (double key in finishedCancellations)
        {
            Destroy(cancellationTimers[key]);
            cancellationTimers.Remove(key);
        }
    }

    private void LateUpdate()
    {
        AdjustHeight();
    }

    private void JobAdd(Vector2Int jobRef)
    {
        Job job = PositionManager.i.GetJobFromRef(jobRef);
        currentJobHandlers.Add(job.handler);
        JobHandler jobHandler = job.handler;
        jobHandler.clientConnected = true;

        jobHandler.onTaskAssignClient += TaskAdd;
        jobHandler.onTasksFinishClient += TaskFinish;
        jobHandler.onTaskCancelClient += TaskCancel;
        jobHandler.onTaskCompleteClient += TaskComplete;
    }

    private void JobRemove(Vector2Int jobRef)
    {
        Job job = PositionManager.i.GetJobFromRef(jobRef);
        currentJobHandlers.Remove(job.handler);
        JobHandler jobHandler = job.handler;
        jobHandler.clientConnected = false;

        jobHandler.onTaskAssignClient -= TaskAdd;
        jobHandler.onTasksFinishClient -= TaskFinish;
        jobHandler.onTaskCancelClient -= TaskCancel;
        jobHandler.onTaskCompleteClient -= TaskComplete;
    }

    private void TaskAdd(int taskId, JobHandler source)
    {
        Task taskInfo = source.GetTaskFromId(taskId);
        int taskIndex = GetTaskIndex(taskInfo, source); // Gets the place to insert the task
        GameObject taskObject = Instantiate(taskPrefab, taskHolder); // Instantaite task object and ui task class
        UITask newUITask = new UITask(taskId, source, taskObject, taskInfo.deadline);
        uiTasks.Insert(taskIndex, newUITask);
        taskObject.transform.SetSiblingIndex(taskIndex);

        PhysTask physTask = taskObject.GetComponent<PhysTask>();
        // replaces deadline with the actual deadline
        string taskText = taskInfo.name.ToString();
        string periodText = gameManager.PeriodToClockString(taskInfo.deadline);
        taskText = taskText.Replace("<deadline>", periodText);
        physTask.SetTaskText(taskText);
        physTask.SetCompleted(false);
    }

    private void TaskFinish(TaskFinishInfo finishInfo, JobHandler source)
    {
        List<Task> finishedTasks = new List<Task>(finishInfo.associatedTasks);
        // The index to place the finish info
        int placeIndex = taskHolder.childCount;
        // The size of the finish info
        float sizeY = 0f;
        // Find the corresponding UI task to this transform. Set the place index if its lower. Add the size to the total size
        List<GameObject> uiObjects = new List<GameObject>();
        foreach (Task task in finishedTasks)
        {
            UITask uiTask = GetUITaskWithID(task.id);
            if (uiTask == null) continue;
            int siblingIndex = uiTask.uiObject.transform.GetSiblingIndex();
            if (siblingIndex < placeIndex)
            {
                placeIndex = siblingIndex;
            }
            sizeY += ((RectTransform)uiTask.uiObject.transform).sizeDelta.y;
            uiObjects.Add(uiTask.uiObject);
        }
        // Instantiate the finish object
        GameObject finishObject = Instantiate(taskResolutionPrefab, taskHolder);
        finishObject.transform.SetSiblingIndex(placeIndex);
        RectTransform finishRT = (RectTransform)finishObject.transform;
        finishRT.sizeDelta = new Vector2(finishRT.sizeDelta.x, sizeY);
        // Delete all ui objects relating to the task finish
        foreach (GameObject obj in uiObjects) Destroy(obj);
        PhysTaskFinish physFinish = finishObject.GetComponent<PhysTaskFinish>();
        physFinish.SetRewardText(finishInfo.reward, finishInfo.strikes);
        string rewardReason = finishInfo.rewardReason.ToString();
        string strikeReason = finishInfo.strikeReason.ToString();
        physFinish.SetTaskText(rewardReason, strikeReason);
    }

    private void TaskCancel(int taskId, JobHandler source)
    {
        foreach (UITask task in uiTasks)
        {
            if (task.taskId == taskId)
            {
                task.uiObject.GetComponent<PhysTask>().Cancel();
                cancellationTimers.Add(Time.realtimeSinceStartupAsDouble + cancelDuration, task.uiObject);
                return;
            }
        }
    }

    private void TaskComplete(int taskId, JobHandler source)
    {
        foreach (UITask task in uiTasks)
        {
            if (task.taskId == taskId)
            {
                task.uiObject.GetComponent<PhysTask>().SetCompleted(true);
                return;
            }
        }
    }

    private int GetTaskIndex(Task task, JobHandler handler)
    {
        int handlerIndex = currentJobHandlers.IndexOf(handler);
        for (int i = 0; i < uiTasks.Count; i++)
        {
            // If the index of the job handler is less than, return i
            int uiTaskIndex = currentJobHandlers.IndexOf(uiTasks[i].handler);
            if (handlerIndex < uiTaskIndex)
            {
                return i;
            }
            // If the handler index is the same as the ui task index, sort by deadlines
            if (handlerIndex == uiTaskIndex)
            {
                if (task.deadline < uiTasks[i].deadline)
                {
                    return i;
                }
                // Sort by ids if the deadlines are the same
                if (task.id < uiTasks[i].taskId)
                {
                    return i;
                }
            }
        }
        // Place at the end if nothing happens
        return uiTasks.Count;
    }

    private UITask GetUITaskWithID(int taskId)
    {
        foreach (UITask task in uiTasks)
        {
            if (task.taskId == taskId) return task;
        }
        return null;
    }

    private void AdjustHeight()
    {
        float newHeight = 0f;
        foreach (Transform element in taskHolder)
        {
            RectTransform rt = (RectTransform)element;
            newHeight += rt.sizeDelta.y;
        }
        RectTransform rect = (RectTransform)transform;
        if (newHeight > minHeight)
        {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);
            return;
        }
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, minHeight);
    }
}
