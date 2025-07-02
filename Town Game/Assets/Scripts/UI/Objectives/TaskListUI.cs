using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskListUI : MonoBehaviour
{
    public GameObject taskPrefab;
    public GameObject taskResolutionPrefab;
    public Transform taskHolder;
    public List<UITask> uiTasks = new List<UITask>();
    private List<JobHandler> currentJobHandlers = new List<JobHandler>();

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
    }

    private void JobAdd(Vector2Int jobRef)
    {
        Job job = PositionManager.i.GetJobFromRef(jobRef);
        currentJobHandlers.Add(job.handler);
        JobHandler jobHandler = job.handler;
        jobHandler.clientConnected = true;

        jobHandler.onTaskAddClient += TaskAdd;
        jobHandler.onTasksFinishClient += TaskFinish;
    }

    private void JobRemove(Vector2Int jobRef)
    {
        Job job = PositionManager.i.GetJobFromRef(jobRef);
        currentJobHandlers.Remove(job.handler);
        JobHandler jobHandler = job.handler;
        jobHandler.clientConnected = false;

        jobHandler.onTaskAddClient -= TaskAdd;
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
        physTask.SetTaskText(taskInfo.name.ToString());
        physTask.SetCompleted(false);
    }

    private void TaskFinish(TaskFinishInfo finishInfo, JobHandler source)
    {

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
            // If the handler index is the same as the ui task index, sort by ids
            if (handlerIndex == uiTaskIndex)
            {
                if (task.deadline < uiTasks[i].deadline)
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
}
