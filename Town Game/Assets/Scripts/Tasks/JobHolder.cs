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
    [Header("Period Settings")]
    [Tooltip("The range of the period blocks from this job holder we can have. x is the beginning bounds, y is the end bounds, and z is the target center")]
    public Vector3Int periodAddRange = new Vector3Int(9, 19, 9);
    public float periodLength = 120f;
    public Color jobColor = Color.white;
    public string[] taskCategories = new string[0];
    public string generalPeriodName = "General Job Stuff";
    public int jobIconIndex;
    RunnerManager runnerManager;
    ScheduleManager scheduleManager;
    GameManager gameManager;

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
        gameManager = FindFirstObjectByType<GameManager>();
        if (!Runner.IsServer) return;
        scheduleManager.OnMasterBlockStart += CheckActiveBlock;
        runnerManager.onPlayerLeave += RemovePlayer;
        OnTasksUpdate += UpdateScheduleTasks;
    }

    public Task CreateTask(string taskName, int category, string room,float timeTaken = 20f, int priority = 0)
    {
        Task newTask = new Task(taskName, category, timeTaken, room, priority);
        activeTasks.Add(newTask);
        OnTasksUpdate?.Invoke();
        return newTask;
    }

    public void RemoveTask(Task taskReference, bool isCompleted)
    {
        activeTasks.Remove(taskReference);
        // Removes the task from trackedblocks
        RemoveFromTracked(taskReference);

        OnTasksUpdate?.Invoke();
        // If the task was completed, send this rpc to the tearout, otherwise send rpc task removal
    }

    void RemoveFromTracked(Task taskRef)
    {
        for (int i = 0; i < trackedBlocks.Count; i++)
        {
            foreach (Task task in trackedBlocks[i].tasks)
            {
                if (taskRef.Equals(task))
                {
                    trackedBlocks[i].tasks.Remove(task);
                    return;
                }
            }
        }
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
        /**
        // Tasks that are already added
        List<Task> culledTasks = new List<Task>();
        // Periods that are not full
        List<int> freePeriods = new List<int>();
        int i = 0;
        foreach (TrackedBlock block in trackedBlocks)
        {
            foreach (Task task in block.tasks)
            {
                culledTasks.Add(task);
            }
            if (!TrackedBlockIsFull(block)) freePeriods.Add(i);
            i++;
        }

        // Blocks to be added to schedule manager
        List<ScheduleBlock> addedBlocks = new List<ScheduleBlock>();
        // Iterate over every task and add to a period
        foreach (Task task in activeTasks)
        {
            // Doesn't add any tasks already added
            if (culledTasks.Contains(task)) continue;

            // Check free periods to add
            if (freePeriods.Count > 0)
            {
                trackedBlocks[freePeriods[0]].tasks.Add(task);
                if (TrackedBlockIsFull(trackedBlocks[freePeriods[0]])) freePeriods.RemoveAt(0); // Removes if its full
                continue;
            }

            // If everything else is full, create a new period
            float newTime = 0f;
        }
        **/

        // Update period names
    }

    // Checks if the current block is within our active blocks. If it is, send this information to the assigned
    void CheckActiveBlock(ScheduleBlock block)
    {

    }

    /// <summary>
    /// Returns true if there is not enough time for every task to be completed in this block.
    /// </summary>
    /// <param name="trackedBlock"></param>
    /// <returns></returns>
    public bool TrackedBlockIsFull(TrackedBlock trackedBlock)
    {
        float timeTotal = trackedBlock.block.length * gameManager.hourLength;
        float addedTime = 0f;
        foreach (Task task in trackedBlock.tasks)
        {
            addedTime += task.secondsTaken;
            if (addedTime >= timeTotal) return true;
        }
        return false;
    }
}
