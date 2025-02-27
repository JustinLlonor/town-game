using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using WebSocketSharp;
using System.Linq;
using UnityEngine.UI;

public class TasksUI : MonoBehaviour
{
    public List<PTask> physTasks = new List<PTask>();
    public RectTransform taskPanel;
    public GameObject container;
    public GameObject physTaskPrefab;
    public Transform taskHolder;
    // For persistent states like patrol
    public Transform stateTaskHolder;
    PlayerRoom pr;
    PlayerManager pm;
    ScheduleManager sm;
    TaskHolder trackedTasks;
    bool uiShowing = false;

    public class PTask
    {
        public GameObject phys;
        public string name;

        public PTask(GameObject phys, string name)
        {
            this.phys = phys; this.name = name;
        }
    }

    private void Awake()
    {
        sm = FindFirstObjectByType<ScheduleManager>();
        pm = FindFirstObjectByType<PlayerManager>();
        pm.OnInstantiatePlayer += GetPlayerReferences;
        //sm.OnBlockChange += PeriodChange;
    }

    void GetPlayerReferences(GameObject player)
    {
        pr = player.GetComponent<PlayerRoom>();
        //pr.OnEnterRoom += OnEnterRoom;
    }

    void OnEnterRoom(MapRoom room)
    {
        if (uiShowing) return;
        GetRoomTasks(room);
    }

    // Show state task ui if the period has no room
    void PeriodChange(ScheduleBlock from, ScheduleBlock to)
    {
        // If free time, clear tasks
        if (to.Equals(ScheduleBlock.None))
        {
            ClearTrackedTasks();
            HideUI();
            return;
        }
        // If period, try to get state tasks
        //if (to.room.IsNullOrEmpty())
        //{
        //    GetStateTasks(to.periodName);
        //    return;
        //}
        // If our current room is equal to the room of the block that was just switched to, get the room tass
        if (pr.currentRoom != null)
        {
            if (pr.currentRoom.roomName == to.room)
            {
                GetRoomTasks(pr.currentRoom);
                return;
            }
        }
        HideUI();
    }

    // Gets task holder of the room and calls get tasks
    void GetRoomTasks(MapRoom room)
    {
        if (sm.dcurrentBlock.Equals(ScheduleBlock.None)) return;
        if (sm.dcurrentBlock.room != room.roomName) return;
        if (room.taskHolder == null)
        {
            Debug.LogWarning("GetRoomTasks called, but no task holder on the room.");
            return;
        }
        if (!room.taskHolder.assigned) return;
        GetTasks(room.taskHolder);
    }

    // Gets task holder of the state and calls get tasks
    void GetStateTasks(string state)
    {
        foreach (Transform child in stateTaskHolder)
        {
            if (child.name == state)
            {
                GetTasks(child.GetComponent<TaskHolder>());
                return;
            }
        }
    }

    // Gets tasks of selected task holder
    void GetTasks(TaskHolder th)
    {
        if (trackedTasks == th) return;
        ShowUI();
        ClearTrackedTasks();
        trackedTasks = th;
        trackedTasks.OnTasksUpdate += UpdateTasks;
        UpdateTasks();
    }

    void UpdateTasks()
    {
        if (trackedTasks == null) return;

        List<PTask> removeList = new List<PTask>();
        // Check if the block exists. If it does, update progress, if it doesn't, destroy
        foreach (PTask task in physTasks)
        {
            Task item = trackedTasks.tasks.SingleOrDefault(i => i.name == task.name);
            if (item == null)
            {
                removeList.Add(task);
                continue;
            }
            task.phys.GetComponent<PhysTask>().SetProgress(item.progress);
        }

        // Check if the task is added to UI, if it isn't then add it
        foreach (Task task in trackedTasks.tasks)
        {
            PTask item = physTasks.SingleOrDefault(i => i.name == task.name);
            if (item == null)
            {
                AddTask(task.name, task.progress);
            }
        }

        // Remove
        foreach (PTask task in removeList)
        {
            Destroy(task.phys);
            physTasks.Remove(task);
        }
    }

    void AddTask(string taskName, float progression = -1f)
    {
        GameObject newTask = Instantiate(physTaskPrefab, taskHolder);
        PhysTask pt = newTask.GetComponent<PhysTask>();
        pt.SetText(taskName);
        if (progression >= 0f) pt.SetProgress(progression);
        physTasks.Add(new PTask(newTask, taskName));
        ContentSizeFitter csf = newTask.GetComponent<ContentSizeFitter>();
        csf.enabled = true;
    }

    void ClearTrackedTasks()
    {
        ClearUI();
        if (trackedTasks == null) return;
        trackedTasks.OnTasksUpdate -= UpdateTasks;
        trackedTasks = null;
    }

    void RemoveTask(string taskName)
    {
        var item = physTasks.SingleOrDefault(i => i.name == taskName);
        if (item != null)
        {
            physTasks.Remove(item);
            return;
        }
        Debug.LogError($"Trying to remove task {taskName}, but it does not exist.");
    }

    void HideUI()
    {
        container.SetActive(false);
    }

    void ShowUI()
    {
        container.SetActive(true);
    }

    void ClearUI()
    {
        foreach (PTask pt in physTasks)
        {
            Destroy(pt.phys);
        }
        physTasks.Clear();
    }
}
