using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskListUI : MonoBehaviour
{
    public GameObject taskPrefab;
    public GameObject taskResolutionPrefab;
    public Transform taskHolder;
    public List<UITask> uiTasks = new List<UITask>();
    private List<Job> currentJobs = new List<Job>();

    [System.Serializable]
    public class UITask
    {
        public int taskId;
        public JobHandler handler;
        public GameObject uiObject;

        public UITask(int taskId, JobHandler handler, GameObject uiObject)
        {
            this.taskId = taskId;
            this.handler = handler;
            this.uiObject = uiObject;
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
        currentJobs.Add(job);
        JobHandler jobHandler = job.handler;
        jobHandler.clientConnected = true;

        jobHandler.onTaskAddClient += TaskAdd;
    }

    private void JobRemove(Vector2Int jobRef)
    {
        Job job = PositionManager.i.GetJobFromRef(jobRef);
        currentJobs.Remove(job);
        JobHandler jobHandler = job.handler;
        jobHandler.clientConnected = false;

        jobHandler.onTaskAddClient -= TaskAdd;
    }

    private void TaskAdd(int taskId, JobHandler source)
    {
        Task taskInfo = source.GetTaskFromId(taskId);
        GameObject taskObject = Instantiate(taskPrefab, taskHolder);
        UITask newUITask = new UITask(taskId, source, taskObject);
        uiTasks.Add(newUITask);
    }
}
