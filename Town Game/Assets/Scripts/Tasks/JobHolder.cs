using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

public class JobHolder : NetworkBehaviour
{
    // All schedule blocks relating to this job, only blocks in the future can be removed or added
    public List<TrackedBlock> trackedBlocks = new List<TrackedBlock>();
    [SerializeField] private string subtext;
    // All tasks that have not been completed
    public List<Task> activeTasks = new List<Task>();
    public TasksUpdate OnTasksUpdate;
    public List<PlayerRef> assignedPlayers = new List<PlayerRef>();
    public ScheduleUI scheduleUI;
    [Header("Period Visuals")]
    public Color jobColor = Color.white;
    public string[] taskCategories = new string[0];
    public string generalPeriodName = "General Job Stuff";
    public int jobIconIndex;
    RunnerManager runnerManager;
    ScheduleManager scheduleManager;

    public delegate void TasksUpdate();

    public class TrackedBlock
    {
        public ScheduleBlock block;
        public List<Task> tasks;

        public TrackedBlock(ScheduleBlock block, List<Task> tasks)
        {
            this.block = block;
            this.tasks = tasks;
        }
    }

    public override void Spawned()
    {
        runnerManager = FindFirstObjectByType<RunnerManager>();
        scheduleManager = FindFirstObjectByType<ScheduleManager>();
        if (!Runner.IsServer) return;
        scheduleManager.OnMasterBlockStart += CheckActiveBlock;
        runnerManager.onPlayerLeave += RemovePlayer;
    }

    public Task CreateTask(string taskName, int category, float timeTaken = 20f)
    {
        Task newTask = new Task(taskName, category, timeTaken);
        activeTasks.Add(newTask);
        OnTasksUpdate?.Invoke();
        return newTask;
    }

    public void RemoveTask(Task taskReference, bool isCompleted)
    {
        activeTasks.Remove(taskReference);
        OnTasksUpdate?.Invoke();
        // If the task was completed, send this rpc to the tearout
    }

    public void SetSubtext(string newSubtext)
    {
        subtext = newSubtext;
    }

    public void SetTrackedBlocks(List<TrackedBlock> blocks)
    {
        trackedBlocks = blocks;
    }

    /// <summary>
    /// Assigns a player to this job
    /// </summary>
    /// <param name="player"></param>
    public void AssignPlayer(PlayerRef player)
    {
        if (assignedPlayers.Contains(player)) return;
        assignedPlayers.Add(player);
        UpdateScheduleTasks();
    }

    /// <summary>
    /// Removes a player from this job
    /// </summary>
    /// <param name="player"></param>
    public void RemovePlayer(PlayerRef player)
    {
        if (!assignedPlayers.Contains(player)) return;
        assignedPlayers.Remove(player);
        UpdateScheduleTasks();
    }

    // Updates the schedule per player for this job.
    private void UpdateScheduleTasks()
    {

    }

    // Checks if the current block is within our active blocks. If it is, send this information to the assigned
    void CheckActiveBlock(ScheduleBlock block)
    {

    }
}
