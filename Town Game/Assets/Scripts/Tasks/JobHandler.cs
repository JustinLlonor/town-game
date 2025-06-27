using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

public class JobHandler : NetworkBehaviour
{
    [Header("Set this to false if this job handler is not associated with a job. Make the Job.cs class automatically remove the job ref from player properties later.")]
    public bool associatedWithJob = true;
    [Header("Settings")]
    // All tasks
    // The amount of tasks assigned to a category it takes to create a new state
    public int stateCreationThreshold = 2;
    public List<PlayerRef> hiredPlayers = new List<PlayerRef>();
    public int groupIndex = 0;
    [Header("Period Settings")]
    [Tooltip("The range of the period blocks from this job holder we can have. x is the beginning bounds, y is the end bounds")]
    public Vector2Int periodAddRange = new Vector2Int(9, 19);
    public float periodLength = 2f;
    public float periodSpacing = 2f;
    public float periodTimePerDay = 4f;
    public int maxDays = 3;
    public Color jobColor = Color.white;
    public string[] taskCategories = new string[0];
    public string generalPeriodName = "General Job Stuff";
    public int jobIconIndex;
    [Networked, Capacity(15)] public NetworkLinkedList<Task> activeTasks => default;
    [Networked, Capacity(15)] public NetworkDictionary<Task, PlayerRef> assignedPlayers => default;

    public JobHandlerEvent OnTaskListUpdate;
    public TaskEvent OnTaskComplete;

    public delegate void JobHandlerEvent();
    public delegate void TaskEvent(Task task);

    RunnerManager runnerManager;
    ScheduleManager scheduleManager;
    GameManager gameManager;
    PlayerManager playerManager;
    AnnouncementManager announcementManager;
    ScheduleUI scheduleUI;
    PositionManager positionManager;

    // Test
    //public Task testTask = new Task("Serve Food", 0, 20f, "Cafeteria");
    //Task recentTask;
    //TaskState recentState;

    private void Update()
    {
        // Test code for reference
        /**
        if (Input.GetKeyDown(KeyCode.U))
        {
            Task newTask = new Task(testTask.name, testTask.category, testTask.secondsTaken, testTask.room);
            recentState = AddClosedState(new List<Task>() { newTask }, "Maintain Cafeteria", new List<PlayerRef>() { Runner.LocalPlayer }, 9f, 1f);
            recentTask = newTask;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (!hiredPlayers.Contains(Runner.LocalPlayer))
            {
                HirePlayer(Runner.LocalPlayer);
            }
            else
            {
                FirePlayer(Runner.LocalPlayer);
            }
        }
        if (Input.GetKeyDown(KeyCode.I)) CompleteTask(recentTask);
        if (Input.GetKeyDown(KeyCode.O))
        {
            Task newTask = new Task(testTask.name, testTask.category, testTask.secondsTaken, testTask.room);
            recentTask = newTask;
            AddTaskToState(newTask, recentState);
        }
        **/
    }

    public override void Spawned()
    {
        runnerManager = FindFirstObjectByType<RunnerManager>();
        scheduleManager = FindFirstObjectByType<ScheduleManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        playerManager = FindFirstObjectByType<PlayerManager>();
        scheduleUI = FindFirstObjectByType<ScheduleUI>();
        announcementManager = FindFirstObjectByType<AnnouncementManager>();
        positionManager = FindAnyObjectByType<PositionManager>();
        if (!Runner.IsServer) return;
        runnerManager.onPlayerLeave += FirePlayer;
    }

    /// <summary>
    /// Assigns a player to this job
    /// </summary>
    /// <param name="player"></param>
    public void HirePlayer(PlayerRef player)
    {
        if (hiredPlayers.Contains(player)) return;
        Debug.Log("Hired player");
        hiredPlayers.Add(player);
        playerManager.AddPlayerToGroup(player, groupIndex);
        if (associatedWithJob)
        {
            positionManager.AddJobProperty(player, positionManager.GetJobHandlerFromRef(this));
        }
    }

    /// <summary>
    /// Removes a player from this job
    /// </summary>
    /// <param name="player"></param>
    public void FirePlayer(PlayerRef player)
    {
        if (associatedWithJob)
        {
            positionManager.RemoveJobProperty(player, positionManager.GetJobHandlerFromRef(this));
        }
        Debug.Log("Firing player");
        if (!hiredPlayers.Contains(player)) return;
        hiredPlayers.Remove(player);
        playerManager.RemovePlayerFromGroup(player, groupIndex);
        // Unassign this player from every task
        List<Task> removalList = new List<Task>();
        foreach (KeyValuePair<Task, PlayerRef> kvp in assignedPlayers)
        {
            if (kvp.Value == player)
            {
                removalList.Add(kvp.Key);
            }
        }
        foreach (Task task in removalList) RemoveTaskAssignment(task);
    }

    /// <summary>
    /// Adds a task and automatically assigns it to a player
    /// </summary>
    /// <param name="task"></param>
    /// <param name="assignedPlayer"></param>
    public void AddTask(Task task)
    {
        activeTasks.Add(task);
        AutoAssignTasks();
        OnTaskListUpdate?.Invoke();
    }

    /// <summary>
    /// Adds a task assigned to the specified player
    /// </summary>
    /// <param name="task"></param>
    /// <param name="assignedPlayer"></param>
    public void AddTask(Task task, PlayerRef assignedPlayer)
    {
        activeTasks.Add(task);
        AssignTask(task, assignedPlayer);
        OnTaskListUpdate?.Invoke();
    }

    public void RemoveTask(Task taskReference)
    {
        if (!activeTasks.Contains(taskReference)) return;
        activeTasks.Remove(taskReference);
        OnTaskListUpdate?.Invoke();
    }

    public Task CompleteTask(Task task)
    {
        if (!activeTasks.Contains(task)) return Task.None;
        int taskIndex = activeTasks.IndexOf(task);
        if (taskIndex == -1)
        {
            Debug.LogError("Task not found!");
            return Task.None;
        }
        Task newTask = activeTasks[taskIndex];
        newTask.isCompleted = true;
        activeTasks.Set(taskIndex, newTask);
        OnTaskComplete?.Invoke(newTask);
        return newTask;
    }

    public void RemoveTaskAssignment(Task task)
    {
        assignedPlayers.Remove(task);
    }

    public void AssignTask(Task task, PlayerRef player)
    {
        if (assignedPlayers.ContainsKey(task)) return;
        assignedPlayers.Add(task, player);
    }

    /// <summary>
    /// Assigns tasks to players automatically
    /// </summary>
    private void AutoAssignTasks()
    {
        if (assignedPlayers.Count == activeTasks.Count) return;
        if (hiredPlayers.Count == 0) return;
        if (hiredPlayers.Count == 1)
        {
            foreach (Task task in activeTasks)
            {
                if (assignedPlayers.ContainsKey(task)) continue;
                AssignTask(task, hiredPlayers[0]);
            }
            return;
        }
        // Set initial task counts
        Dictionary<PlayerRef, int> taskCounts = new Dictionary<PlayerRef, int>();
        foreach (PlayerRef player in hiredPlayers)
        {
            taskCounts.Add(player, 0);
        }
        foreach (KeyValuePair<Task, PlayerRef> kvp in assignedPlayers)
        {
            taskCounts[kvp.Value]++;
        }
        // Assign tasks to the lowest player
        foreach (Task task in activeTasks)
        {
            if (assignedPlayers.ContainsKey(task)) continue;
            int lowestValue = 999;
            PlayerRef lowestPlayer = PlayerRef.None;
            foreach (KeyValuePair<PlayerRef, int> kvp in taskCounts)
            {
                if (kvp.Value < lowestValue)
                {
                    lowestValue = kvp.Value;
                    lowestPlayer = kvp.Key;
                }
            }
            if (lowestPlayer == PlayerRef.None) return;
            // Increase task count for the lowest player on this iteration
            taskCounts[lowestPlayer]++;
            AssignTask(task, lowestPlayer);
        }
        
    }
}
