using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

public class JobHandler : NetworkBehaviour
{
    // All schedule blocks relating to this job, only blocks in the future can be removed or added
    [SerializeField] private string subtext;
    // All tasks that have not been completed
    public List<Task> activeTasks = new List<Task>();
    // The amount of tasks assigned to a category it takes to create a new state
    public int stateCreationThreshold = 2;
    public TasksUpdate OnTasksUpdate;
    public List<PlayerRef> hiredPlayers = new List<PlayerRef>();
    public ScheduleUI scheduleUI;
    [Header("Period Settings")]
    [Tooltip("The range of the period blocks from this job holder we can have. x is the beginning bounds, y is the end bounds, and z is the target center")]
    public Vector3Int periodAddRange = new Vector3Int(9, 19, 9);
    public float periodLength = 120f;
    public Color jobColor = Color.white;
    public string[] taskCategories = new string[0];
    public string generalPeriodName = "General Job Stuff";
    public int jobIconIndex;
    private List<TaskState> states = new List<TaskState>();
    RunnerManager runnerManager;
    ScheduleManager scheduleManager;
    GameManager gameManager;

    public delegate void TasksUpdate();

    public class TaskState
    {
        public string category;
        public string room;
        public List<Task> tasks;
        public bool closed;

        public TaskState(string category, string room, List<Task> tasks, bool closed)
        {
            this.category = category;
            this.room = room;
            this.tasks = tasks;
            this.closed = closed;
        }

        public void UpdateRoomName()
        {
            if (tasks.Count == 0)
            {
                room = "";
                return;
            }

            // Count of every room string in the task list
            Dictionary<string, int> roomCounts = new Dictionary<string, int>();
            foreach (Task task in tasks)
            {
                if (roomCounts.ContainsKey(task.room))
                {
                    roomCounts[task.room]++;
                }
                else
                {
                    roomCounts.Add(task.room, 1);
                }
            }

            string highestRoom = "";
            int highestCount = 0;
            foreach (KeyValuePair<string, int> pair in roomCounts)
            {
                if (pair.Value > highestCount)
                {
                    highestCount = pair.Value;
                    highestRoom = pair.Key;
                }
            }

            room = highestRoom;
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

    /// <summary>
    /// Adds the specified tasks to the states
    /// </summary>
    /// <param name="tasks">The list of tasks to be added</param>
    /// <param name="stateClosed">If the state is closed or not. If it is closed, the state is created with all of the tasks</param>
    /// <param name="categoryName">Only used if the state is closed. Sets the state category name</param>
    private void AddTasksToStates(List<Task> tasks, bool stateClosed = false, string categoryName = "")
    {
        if (stateClosed)
        {
            AddState(categoryName, new List<Task>(tasks), false);
            return;
        }

        List<Task> remainingTasks = new List<Task>(tasks);

        // Find compatible states to add to
        for (int i = 0; i < states.Count; i++)
        {
            if (IsStateFull(states[i])) continue; // If the state has no more time for new tasks
            if (states[i].closed) continue; // If the state is closed/is immutable, return
            for (int t = 0; t < remainingTasks.Count; t++) // Check every remainng task on this state to see if they can add
            {
                if (taskCategories[remainingTasks[t].category] == states[i].category && (!IsStateFull(states[i]))) // If the category of the current state is the same as the current task, and its not full, add this task
                {
                    states[i].tasks.Add(tasks[t]); // Add this task
                    remainingTasks.Remove(tasks[t]);
                    states[i].UpdateRoomName();
                    t--;
                }
            }
        }

        Dictionary<int, List<Task>> categoryTasks = new Dictionary<int, List<Task>>();
        // Convert reaminingTasks to category key value pairs
        if (remainingTasks.Count > 0)
        {
            for (int i = 0; i < remainingTasks.Count; i++)
            {
                Task task = remainingTasks[i];
                if (categoryTasks.ContainsKey(task.category))
                {
                    categoryTasks[task.category].Add(task);
                }
                else
                {
                    categoryTasks[task.category] = new List<Task>();
                }
            }
        }

        float maxTime = periodLength / gameManager.hourLength;
        // Adds each state
        foreach (KeyValuePair<int, List<Task>> ct in categoryTasks)
        {
            List<Task> taskList = new List<Task>();
            float addedTime = 0f;
            foreach (Task task in ct.Value)
            {
                taskList.Add(task);
                addedTime += task.secondsTaken; // If its full, create a new state for the same category
                if (addedTime > maxTime)
                {
                    AddState(ct.Key, new List<Task>(taskList), false);
                    addedTime = 0f;
                    taskList.Clear();
                }
            }
            if (taskList.Count > 0)
            {
                AddState(ct.Key, new List<Task>(taskList), false);
            }
        }
    }

    private void RemoveTasksFromStates(List<Task> tasks)
    {
        // If a state is removed here, call AddtasksToStates
    }

    // State creating methods
    private void AddState(int category, List<Task> tasks, bool closed)
    {
        TaskState newState = new TaskState(taskCategories[category], "", tasks, closed);
        newState.UpdateRoomName();
        states.Add(newState);
    }
    private void AddState(string category, List<Task> tasks, bool closed)
    {
        TaskState newState = new TaskState(category, "", tasks, closed);
        newState.UpdateRoomName();
        states.Add(newState);
    }

    public void AddTasks(List<Task> tasks)
    {
        foreach (Task task in tasks)
        {
            activeTasks.Add(task);
        }
        OnTasksUpdate?.Invoke();

        AddTasksToStates(tasks);
    }

    public void AddClosedState(List<Task> tasks, string categoryName)
    {
        AddTasksToStates(tasks, true, categoryName);
    }

    public void RemoveTask(Task taskReference, bool isCompleted)
    {
        activeTasks.Remove(taskReference);

        OnTasksUpdate?.Invoke();
        // Remove the task from states

        // If the task was completed, send this rpc to the tearout, otherwise send rpc task removal
    }

    public void SetSubtext(string newSubtext)
    {
        subtext = newSubtext;
    }

    /// <summary>
    /// Assigns a player to this job
    /// </summary>
    /// <param name="player"></param>
    public void AssignPlayer(PlayerRef player)
    {
        if (hiredPlayers.Contains(player)) return;
        hiredPlayers.Add(player);
        UpdateScheduleTasks();
    }

    /// <summary>
    /// Removes a player from this job
    /// </summary>
    /// <param name="player"></param>
    public void RemovePlayer(PlayerRef player)
    {
        if (!hiredPlayers.Contains(player)) return;
        hiredPlayers.Remove(player);
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

    bool IsStateFull(TaskState state)
    {
        float totalTime = 0f;
        float maxTime = periodLength / gameManager.hourLength;
        foreach (Task task in state.tasks)
        {
            totalTime += task.secondsTaken;
            if (totalTime > maxTime) return true;
        }
        return false;
    }
}
