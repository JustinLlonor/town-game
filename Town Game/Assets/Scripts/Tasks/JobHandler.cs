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
    public List<Task> activeTasks;

    public JobHandlerEvent OnTasksUpdate;

    public delegate void JobHandlerEvent();

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
    }

    public void AddTasks(List<Task> tasks)
    {
        Debug.Log("Adding tasks");
        foreach (Task task in tasks)
        {
            activeTasks.Add(task);
        }
        OnTasksUpdate?.Invoke();
    }

    public void RemoveTask(Task taskReference)
    {
        if (!activeTasks.Contains(taskReference)) return;
        activeTasks.Remove(taskReference);
        OnTasksUpdate?.Invoke();
    }

    public void CompleteTask(Task task)
    {
        if (!activeTasks.Contains(task)) return;
        int taskIndex = activeTasks.IndexOf(task);
        activeTasks[taskIndex].isCompleted = true;
    }
}
