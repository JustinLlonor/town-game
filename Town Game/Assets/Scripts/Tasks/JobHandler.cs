using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

public class JobHandler : NetworkBehaviour
{
    [Header("Settings")]
    public List<PlayerRef> hiredPlayers = new List<PlayerRef>();
    public int groupIndex = 0;
    public Color jobColor = Color.white;
    public int jobIconIndex;
    // Use change detectors with these task lists for the UI stuff
    // The list of all tasks that are currently active
    [Networked, Capacity(10)] public NetworkLinkedList<Task> activeTasks => default;
    private List<int> previousActiveTasks = new List<int>();
    // The list of all tasks that have been resolved. Tasks that are not complete within resolvedTasks are cancelled.
    [Networked, Capacity(10)] public NetworkLinkedList<Task> resolvedTasks => default;
    private List<int> previousResolvedTasks = new List<int>();
    // The players that are assigned to each task
    [Networked, Capacity(10)] public NetworkDictionary<int, PlayerRef> assignedPlayers => default;
    // If this client is connected to this job handler;
    [HideInInspector] public bool clientConnected = false;
    private Dictionary<float, List<int>> taskDeadlines = new Dictionary<float, List<int>>();

    public JobHandlerEvent OnTaskListUpdate; // executed on server
    public TaskEvent OnTaskCompleteServer;
    public TaskEvent OnTaskCancelServer;
    public MultipleTaskEvent OnTasksFinishServer;
    public TaskEvent onTaskAddClient;
    public TaskEvent onTaskCompleteClient;
    public TaskEvent onTaskCancelClient;
    public MultipleTaskEvent onTasksFinishClient;

    public delegate void JobHandlerEvent();
    public delegate void TaskEvent(int taskId, JobHandler source);
    public delegate void MultipleTaskEvent(int[] taskIds, JobHandler source);

    GameManager gameManager;
    RunnerManager runnerManager;
    PlayerManager playerManager;
    PositionManager positionManager;
    ChangeDetector changeDetector;

    public override void Spawned()
    {
        runnerManager = FindFirstObjectByType<RunnerManager>();
        playerManager = FindFirstObjectByType<PlayerManager>();
        positionManager = FindAnyObjectByType<PositionManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (!Runner.IsServer) return;
        runnerManager.onPlayerLeave += FirePlayer;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer) return;
        CheckTaskDeadlines();
    }

    public override void Render()
    {
        if (!clientConnected) return;
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(resolvedTasks):
                    ClientResolvedTaskEvent();
                    break;
                case nameof(activeTasks):
                    ClientActiveTaskEvent();
                    break;
            }
        }
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
        //if (associatedWithJob)
        positionManager.AddJobProperty(player, positionManager.GetJobHandlerFromRef(this));
        AutoAssignTasks();
    }

    /// <summary>
    /// Removes a player from this job
    /// </summary>
    /// <param name="player"></param>
    public void FirePlayer(PlayerRef player)
    {
        //if (associatedWithJob)
        positionManager.RemoveJobProperty(player, positionManager.GetJobHandlerFromRef(this));
        Debug.Log("Firing player");
        if (!hiredPlayers.Contains(player)) return;
        hiredPlayers.Remove(player);
        playerManager.RemovePlayerFromGroup(player, groupIndex);
        // Unassign this player from every task
        List<int> removalList = new List<int>();
        foreach (KeyValuePair<int, PlayerRef> kvp in assignedPlayers)
        {
            if (kvp.Value == player)
            {
                removalList.Add(kvp.Key);
            }
        }
        foreach (int task in removalList) RemoveTaskAssignment(task);
    }

    /// <summary>
    /// Adds a new incomplete task and automatically assigns it to a player. To be called mostly when the day starts.
    /// </summary>
    /// <param name="name">The name of the task</param>
    /// <param name="deadline">The time, in game time, when the task will finish and assessed for rewards/strikes</param>
    /// <param name="location">The location of the task. Will automatically tell the player which room it is in</param>
    /// <returns></returns>
    public int AddTask(string name, float deadline, Vector3 location)
    {
        Task task = new Task(name, deadline, location, false);
        activeTasks.Add(task);
        AutoAssignTasks();
        OnTaskListUpdate?.Invoke();
        AddTaskDeadline(task);
        return task.id;
    }

    public int AddTask(string name, float deadline, Vector3 location, PlayerRef player)
    {
        Task task = new Task(name, deadline, location, false);
        activeTasks.Add(task);
        AutoAssignTasks();
        OnTaskListUpdate?.Invoke();
        return task.id;
    }

    public void RemoveTask(Task taskReference)
    {
        if (!activeTasks.Contains(taskReference)) return;
        activeTasks.Remove(taskReference);
        RemoveTaskFromDeadlines(taskReference);
        OnTaskListUpdate?.Invoke();
    }

    /// <summary>
    /// Marks the task as complete and moves it to the resolvedTasks list
    /// </summary>
    /// <param name="taskId"></param>
    public void CompleteTask(int taskId, int reward)
    {
        Task taskObject = GetActiveTask(taskId);
        int taskIndex = activeTasks.IndexOf(taskObject);
        if (taskIndex != -1) return;
        if (taskObject.Equals(Task.None)) return;
        Task newTask = taskObject;
        newTask.isCompleted = true;
        activeTasks.Set(taskIndex, newTask);
        ResolveTask(newTask.id);
        OnTaskCompleteServer?.Invoke(taskId, this);
    }

    /// <summary>
    /// Moves the incomplete task from the active list to the resolved list
    /// </summary>
    /// <param name="taskId"></param>
    public void CancelTask(int taskId)
    {
        Task taskObject = GetActiveTask(taskId);
        if (taskObject.Equals(Task.None)) return;
        activeTasks.Remove(taskObject);
        resolvedTasks.Add(taskObject);
        RemoveTaskFromDeadlines(taskObject);
        OnTaskCancelServer?.Invoke(taskId, this);
    }

    /// <summary>
    /// Unassigns this task from anyone who may have it
    /// </summary>
    /// <param name="task"></param>
    public void RemoveTaskAssignment(int task)
    {
        assignedPlayers.Remove(task);
    }

    /// <summary>
    /// Assigns a player to the specfied task
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="player"></param>
    public void AssignTask(int taskId, PlayerRef player)
    {
        if (assignedPlayers.ContainsKey(taskId))
        {
            assignedPlayers.Set(taskId, player);
            return;
        }
        assignedPlayers.Add(taskId, player);
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
            // Assign all unassinged tasks to the player
            foreach (Task task in activeTasks)
            {
                if (assignedPlayers.ContainsKey(task.id)) continue;
                AssignTask(task.id, hiredPlayers[0]);
            }
            return;
        }
        // Set initial task counts
        Dictionary<PlayerRef, int> taskCounts = new Dictionary<PlayerRef, int>();
        foreach (PlayerRef player in hiredPlayers)
        {
            taskCounts.Add(player, 0);
        }
        foreach (KeyValuePair<int, PlayerRef> kvp in assignedPlayers)
        {
            taskCounts[kvp.Value]++;
        }
        // Assign tasks to the lowest player
        foreach (Task task in activeTasks)
        {
            if (assignedPlayers.ContainsKey(task.id)) continue;
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
            AssignTask(task.id, lowestPlayer);
        }
    }

    /// <summary>
    /// Gets the task from the active task list
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public Task GetActiveTask(int taskId)
    {
        foreach (Task task in activeTasks)
        {
            if (task.id == taskId) return task;
        }
        return Task.None;
    }

    /// <summary>
    /// Gets the task from the resolved task list
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public Task GetResolvedTask(int taskId)
    {
        foreach (Task task in resolvedTasks)
        {
            if (task.id == taskId) return task;
        }
        return Task.None;
    }

    /// <summary>
    /// Gets a task from both the resolved and active task lists
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public Task GetTaskFromId(int taskId)
    {
        foreach (Task task in activeTasks)
        {
            if (task.id ==  taskId) return task;
        }
        foreach (Task task in resolvedTasks)
        {
            if (task.id == taskId) return task;
        }
        return Task.None;
    }

    private void ResolveTask(int taskId)
    {
        Task resolvedTask = GetActiveTask(taskId);
        if (resolvedTask.Equals(Task.None)) return;
        activeTasks.Remove(resolvedTask);
        // Deletes the oldest resolved task
        if (resolvedTasks.Count == resolvedTasks.Capacity)
        {
            resolvedTasks.Remove(resolvedTasks[0]);
        }
        resolvedTasks.Add(resolvedTask);
    }

    private void ClientActiveTaskEvent()
    {
        // Find the newtasks
        List<Task> newActiveTasks = new List<Task>();
        foreach (Task task in activeTasks)
        {
            // If a task's id isn't in a previous check, it must be new
            if (!previousActiveTasks.Contains(task.id))
            {
                newActiveTasks.Add(task);
            }
        }
        // Invoke the client events
        foreach (Task task in newActiveTasks)
        {
            onTaskAddClient?.Invoke(task.id, this);
        }
        // Set the new previous tasks to be a list containing the ids of the current resolved tasks
        previousActiveTasks.Clear();
        foreach (Task task in resolvedTasks)
        {
            previousActiveTasks.Add(task.id);
        }
    }

    private void ClientResolvedTaskEvent()
    {
        // Find the new resolved tasks
        List<Task> newResolvedTasks = new List<Task>();
        foreach (Task task in resolvedTasks)
        {
            // If a task's id isn't in a previous check, it must be new
            if (!previousResolvedTasks.Contains(task.id))
            {
                newResolvedTasks.Add(task);
            }
        }
        // Invoke the client events
        foreach (Task task in newResolvedTasks)
        {
            if (task.isCompleted)
            {
                onTaskCompleteClient?.Invoke(task.id, this);
            }
            else
            {
                onTaskCancelClient?.Invoke(task.id, this);
            }
        }
        // Set the new previous resolved tasks to be a list containing the ids of the current resolved tasks
        previousResolvedTasks.Clear();
        foreach (Task task in resolvedTasks)
        {
            previousResolvedTasks.Add(task.id);
        }
    }

    // Adds a task to the deadline dictionary check
    private void AddTaskDeadline(Task task)
    {
        if (taskDeadlines.ContainsKey(task.deadline))
        {
            taskDeadlines[task.deadline].Add(task.id);
            return;
        }
        taskDeadlines.Add(task.deadline, new List<int>() { task.id });
    }

    private void RemoveTaskFromDeadlines(Task task)
    {
        List<float> removalList = new List<float>();
        foreach (var kvp in taskDeadlines)
        {
            if (kvp.Value.Contains(task.id))
            {
                taskDeadlines[kvp.Key].Remove(task.id);
                removalList.Add(kvp.Key);
            }
        }
        foreach (float key in removalList) taskDeadlines.Remove(key);
    }

    private void CheckTaskDeadlines()
    {
        // Checks the task deadlines to see if they passed or have no more tasks
        List<float> finishedDeadlines = new List<float>();
        foreach (var kvp in taskDeadlines)
        {
            if (gameManager.gameTime > kvp.Key || kvp.Value.Count == 0)
            {
                finishedDeadlines.Add(kvp.Key);
            }
        }
        // Removes the task deadlines and invokes the delegate if it has more than 1 task
        foreach (float deadline in finishedDeadlines)
        {
            List<int> finishedTasks = taskDeadlines[deadline];
            taskDeadlines.Remove(deadline);
            if (finishedTasks.Count == 0) continue;
            OnTasksFinishServer?.Invoke(finishedTasks.ToArray(), this);
            //TODO: Make this send groups of info only to the players assigned with their specific tasks.
            //But this is ok if theres only 1 player doing the task stuff
            foreach (PlayerRef player in hiredPlayers)
            {
                RPC_TasksFinish(player, finishedTasks.ToArray());
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_TasksFinish([RpcTarget] PlayerRef player, int[] finishedTasks)
    {
        onTasksFinishClient?.Invoke(finishedTasks, this);
    }
}