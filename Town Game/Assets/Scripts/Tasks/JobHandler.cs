using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

public class JobHandler : NetworkBehaviour
{
    // All tasks
    public Dictionary<Task, TaskState> activeTasks = new Dictionary<Task, TaskState>();
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

    private List<TaskState> states = new List<TaskState>();
    private Dictionary<TaskState, ScheduleBlock> taskBlocks = new Dictionary<TaskState, ScheduleBlock>();

    public JobHandlerEvent OnTasksUpdate;
    public StatesEvent OnStatesAdd;
    public StatesEvent OnStatesRemove;
    public StateEvent OnStateModify;
    public StateEvent OnClosedStateStart;
    /// <summary>
    /// Called when a closed state has been removed, as a result of a player being fired, or a closed state ending. The job behaviour decides what to do with this.
    /// </summary>
    public StateEvent OnClosedStateEnd;
    public delegate void JobHandlerEvent();
    public delegate void StatesEvent(TaskState[] states);
    public delegate void StateEvent(TaskState state);

    RunnerManager runnerManager;
    ScheduleManager scheduleManager;
    GameManager gameManager;
    PlayerManager playerManager;
    AnnouncementManager announcementManager;
    ScheduleUI scheduleUI;

    // Test
    public Task testTask = new Task("Serve Food", 0, 20f, "Cafeteria");
    Task recentTask;
    TaskState recentState;

    private void Update()
    {
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
    }

    /// <summary>
    /// Stores tasks, general summary, and room of a potential schedule block
    /// </summary>
    public class TaskState
    {
        public string category;
        public string room;
        public List<Task> tasks;
        public bool closed;
        public bool storedState;

        public TaskState(string category, string room, List<Task> tasks, bool closed, bool storedState = true)
        {
            this.category = category;
            this.room = room;
            this.tasks = tasks;
            this.closed = closed;
            this.storedState = storedState;
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

        /// <summary>
        /// Returns the length of a period with this many tasks, given the hour length
        /// </summary>
        /// <param name="hourLength"></param>
        /// <returns></returns>
        public float GetPeriodLength(float hourLength)
        {
            float output = 0f;
            foreach (Task task in tasks)
            {
                output += task.secondsTaken / hourLength;
            }
            return output;
        }

        /// <summary>
        /// Returns all incomplete tasks of this task state
        /// </summary>
        /// <returns></returns>
        public List<Task> GetIncompleteTasks()
        {
            if (tasks == null) return new List<Task>();
            List<Task> output = new List<Task>();
            foreach (Task task in tasks)
            {
                if (!task.isCompleted) output.Add(task);
            }
            return output;
        }
    }

    public override void Spawned()
    {
        runnerManager = FindFirstObjectByType<RunnerManager>();
        scheduleManager = FindFirstObjectByType<ScheduleManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        playerManager = FindFirstObjectByType<PlayerManager>();
        scheduleUI = FindFirstObjectByType<ScheduleUI>();
        announcementManager = FindFirstObjectByType<AnnouncementManager>();
        if (!Runner.IsServer) return;
        scheduleManager.OnMasterBlockStart += CheckActiveBlock;
        runnerManager.onPlayerLeave += FirePlayer;
        gameManager.OnChangeDay += CheckOverflowStates;
        scheduleManager.OnMasterBlockEnd += OnPeriodEnd;
        OnStatesAdd += TryDeployStates;
        OnStatesRemove += OnRemoveStates;
        OnStateModify += ModifyState;
        runnerManager.onPlayerLeave += OnPlayerLeave;
        // Test code delete later
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
        CheckOverflowStates();
    }

    /// <summary>
    /// Removes a player from this job
    /// </summary>
    /// <param name="player"></param>
    public void FirePlayer(PlayerRef player)
    {
        Debug.Log("Firing player");
        if (!hiredPlayers.Contains(player)) return;
        hiredPlayers.Remove(player);
        playerManager.RemovePlayerFromGroup(player, groupIndex);
        RemovePlayerFromDeployed(player);
    }

    public void StrikePlayer(PlayerRef player)
    {

    }

    public void AddTasks(List<Task> tasks)
    {
        Debug.Log("Adding tasks");
        foreach (Task task in tasks)
        {
            activeTasks.Add(task, null);
        }
        OnTasksUpdate?.Invoke();

        AddTasksToStates(tasks);
    }

    public void RemoveTask(Task taskReference)
    {
        if (!activeTasks.ContainsKey(taskReference)) return;
        activeTasks.Remove(taskReference);
        OnTasksUpdate?.Invoke();
        // Remove the task from states
        RemoveTasksFromStates(new List<Task>() { taskReference });
    }

    public void CompleteTask(Task task)
    {
        if (!activeTasks.ContainsKey(task)) return;
        int taskIndex = activeTasks[task].tasks.IndexOf(task);
        activeTasks[task].tasks[taskIndex].isCompleted = true;
        OnStateModify?.Invoke(activeTasks[task]);
    }

    /// <summary>
    /// Automatically adds a closed state, then finds the most optimal time and player
    /// </summary>
    /// <param name="tasks"></param>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public TaskState AddClosedState(List<Task> tasks, string categoryName)
    {
        TaskState newState = AddState(categoryName, new List<Task>(tasks), true, true);
        foreach (Task task in tasks) activeTasks.Add(task, newState);
        OnTasksUpdate?.Invoke();
        OnStatesAdd?.Invoke(new TaskState[] { newState });
        return newState;
    }

    /// <summary>
    /// Adds a closed state to the specified time and players
    /// </summary>
    /// <param name="tasks"></param>
    /// <param name="categoryName"></param>
    /// <param name="players"></param>
    /// <param name="time"></param>
    /// <param name="periodLength"></param>
    /// <returns></returns>
    public TaskState AddClosedState(List<Task> tasks, string categoryName, List<PlayerRef> players, float time, float periodLength)
    {
        List<PlayerRef> assignedPlayers = new List<PlayerRef>();
        foreach (PlayerRef player in players)
        {
            if (hiredPlayers.Contains(player))
            {
                Debug.Log("Added assiged player");
                assignedPlayers.Add(player);
            }
            else
            {
                Debug.LogError("Trying to assign a closed state to a player who isn't hired!");
                return null;
            }
        }
        TaskState newState = AddState(categoryName, new List<Task>(tasks), true, false);
        foreach (Task task in tasks) activeTasks.Add(task, newState);
        DeployTaskBlock(newState, time, periodLength, assignedPlayers);
        return newState;
    }

    public void AddTaskToState(Task task, TaskState state) 
    {
        if (state.tasks == null) state.tasks = new List<Task>();
        state.tasks.Add(task);
        activeTasks.Add(task, state);
        OnStateModify?.Invoke(state);
    }

    /// <summary>
    /// Not programmed yet!
    /// </summary>
    /// <param name="state"></param>
    public void RemoveClosedState(TaskState state)
    {

    }

    /// <summary>
    /// Sends schedule subtext to a player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="subtext"></param>
    /// <param name="insertedIndex"></param>
    public void SendSubtext(PlayerRef player, string subtext, int insertedIndex)
    {

    }

    // State behaviours
    /// <summary>
    /// Adds the specified tasks to the states
    /// </summary>
    /// <param name="tasks">The list of tasks to be added</param>
    /// <param name="stateClosed">If the state is closed or not. If it is closed, the state is created with all of the tasks</param>
    /// <param name="categoryName">Only used if the state is closed. Sets the state category name</param>
    private void AddTasksToStates(List<Task> tasks)
    {
        List<Task> remainingTasks = new List<Task>(tasks);
        List<TaskState> addedStates = new List<TaskState>();
        List<TaskState> modifiedStates = new List<TaskState>();

        // Find compatible states to add to
        for (int i = 0; i < states.Count; i++)
        {
            if (IsStateFull(states[i])) continue; // If the state has no more time for new tasks
            if (states[i].closed) continue; // If the state is closed/is immutable, return
            if (taskBlocks.ContainsKey(states[i]))
            {
                if (gameManager.currentPeriod > taskBlocks[states[i]].time)
                {
                    continue; // If the deployed block has already passed
                }
            }
            for (int t = 0; t < remainingTasks.Count; t++) // Check every remainng task on this state to see if they can add
            {
                if (taskCategories[remainingTasks[t].category] == states[i].category && (!IsStateFull(states[i]))) // If the category of the current state is the same as the current task, and its not full, add this task
                {
                    states[i].tasks.Add(tasks[t]); // Add this task
                    activeTasks[tasks[t]] = states[i];
                    remainingTasks.Remove(tasks[t]);
                    if (!modifiedStates.Contains(states[i])) modifiedStates.Add(states[i]);
                    states[i].UpdateRoomName();
                    t--;
                }
            }
        }

        foreach (TaskState state in modifiedStates) OnStateModify?.Invoke(state);

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
                    categoryTasks[task.category] = new List<Task>() { task };
                    Debug.Log("Category task");
                }
            }
        }

        float maxTime = periodLength * gameManager.hourLength;
        // Adds each state
        foreach (KeyValuePair<int, List<Task>> ct in categoryTasks)
        {
            Debug.Log("adding states");
            List<Task> taskList = new List<Task>();
            float addedTime = 0f;
            foreach (Task task in ct.Value)
            {
                taskList.Add(task);
                addedTime += task.secondsTaken; // If its full, create a new state for the same category
                if (addedTime > maxTime)
                {
                    TaskState newState = AddState(ct.Key, new List<Task>(taskList), false);
                    addedTime = 0f;
                    taskList.Clear();
                    addedStates.Add(newState); // Add the created  state
                }
            }
            if (taskList.Count > 0)
            {
                TaskState newState = AddState(ct.Key, new List<Task>(taskList), false);
                Debug.Log("Added state");
                addedStates.Add(newState);
                foreach (Task task in taskList) activeTasks[task] = newState;
            }
        }

        if (addedStates.Count > 0) OnStatesAdd?.Invoke(addedStates.ToArray());
    }

    /// <summary>
    /// Removes the specified tasks from states
    /// </summary>
    /// <param name="tasks"></param>
    private void RemoveTasksFromStates(List<Task> tasks)
    {
        List<TaskState> statesToRemove = new List<TaskState>();
        List<TaskState> modifiedStates = new List<TaskState>();
        List<Task> remainingTasks = new List<Task>(tasks); // Tasks that are yet to be removed

        for (int i = 0; i < states.Count; i++)
        {
            TaskState currentState = states[i];
            for (int o = 0; o < currentState.tasks.Count; o++)
            {
                if (remainingTasks.Contains(currentState.tasks[o]))
                {
                    states[i].tasks.RemoveAt(o);
                    if (!modifiedStates.Contains(states[i])) modifiedStates.Add(states[i]);
                    remainingTasks.Remove(currentState.tasks[o]);
                    o--;
                }
            }

            if (states[i].tasks.Count == 0 && (!states[i].closed))
            {
                statesToRemove.Add(states[i]);
                if (modifiedStates.Contains(states[i])) modifiedStates.Remove(states[i]);
                i--;
            }
        }

        if (statesToRemove.Count > 0) OnStatesRemove?.Invoke(statesToRemove.ToArray());
        RemoveStates(statesToRemove);
        foreach (TaskState state in modifiedStates) OnStateModify?.Invoke(state);
        CheckOverflowStates();
    }

    // State creating methods
    private TaskState AddState(int category, List<Task> tasks, bool closed)
    {
        TaskState newState = new TaskState(taskCategories[category], "", tasks, closed);
        newState.UpdateRoomName();
        states.Add(newState);
        return newState;
    }

    private TaskState AddState(string category, List<Task> tasks, bool closed, bool storedState)
    {
        TaskState newState = new TaskState(category, "", tasks, closed, storedState);
        newState.UpdateRoomName();
        if (storedState) states.Add(newState);
        return newState;
    }

    private void RemoveStates(List<TaskState> stateList)
    {
        foreach (TaskState state in stateList)
        {
            states.Remove(state);
        }
    }

    /**
     * When a task is removed, everything is unaffected until a state is removed
     * When a state is removed or added, it is added as a schedule block if and only if it has more tasks than the threshold
     * When a state is modified, it is checked for task threshold before being added as a block
    **/

    /// <summary>
    /// Attempts to deploy an array of states
    /// </summary>
    /// <param name="states"></param>
    private void TryDeployStates(TaskState[] states)
    {
        // Code to add the state to a player's schedule as a schedule block
        // Checks every schedule for an available space near optimal time
        foreach (TaskState state in states)
        {
            // Gets the best player with the best time, add deploy a task block for them
            PlayerRef bestFitPlayer = PlayerRef.None;
            float bestFitTime = Mathf.Infinity;
            foreach (PlayerRef player in hiredPlayers)
            {
                float playerAvailability = GetFirstAvailableTime(player, periodLength); // The nearest avaiable time for this player
                if (playerAvailability == -1f) continue; // If this player is not available at all, don't log them
                if (playerAvailability < bestFitTime)
                {
                    bestFitPlayer = player;
                    bestFitTime = playerAvailability;
                }
            }
            if (bestFitPlayer == PlayerRef.None) continue;
            DeployTaskBlock(state, bestFitTime, periodLength, bestFitPlayer);
        }
    }

    /// <summary>
    /// Check for any states that aren't deployed and try to deploy them
    /// </summary>
    private void CheckOverflowStates()
    {
        List<TaskState> overflow = GetOverflowStates();

        if (overflow.Count > 0) TryDeployStates(overflow.ToArray());
    }

    private List<TaskState> GetOverflowStates()
    {
        List<TaskState> overflow = new List<TaskState>();
        foreach (TaskState state in states)
        {
            if (!taskBlocks.ContainsKey(state)) overflow.Add(state); // Creates the list of overflow states
        }
        return overflow;
    }

    private void OnRemoveStates(TaskState[] states)
    {
        foreach (TaskState state in states)
        {
            if (!taskBlocks.ContainsKey(state)) continue;
            scheduleManager.RemoveBlock(taskBlocks[state]);
            Debug.Log("1");
            taskBlocks.Remove(state);
        }
        // Code to remove the state and shift other states
    }

    private void ModifyState(TaskState state)
    {
        if (!taskBlocks.ContainsKey(state)) return;
        ScheduleBlock block = taskBlocks[state];
        if (block.GetEquivalentBlockInSchedule(scheduleManager.currentMasterBlocks).Equals(ScheduleBlock.None)) return;
        Debug.Log("Modfy state info");
        SendStateInfoToPlayers(state, block);
        // Send the info to assigned tearout
    }

    private void DeployTaskBlock(TaskState state, float time, float length, PlayerRef player)
    {
        Debug.Log("Task block deployed");
        if (!taskBlocks.ContainsKey(state))
        {
            ScheduleBlock sBlock = scheduleManager.AddBlock(state.category, state.room, time, length, jobColor, new List<PlayerRef>() { player });
            taskBlocks.Add(state, sBlock);
        } else
        {
            List<PlayerRef> assignedPlayers = taskBlocks[state].assignedPlayers;
            assignedPlayers.Add(player);
            scheduleManager.RemoveBlock(taskBlocks[state]);
            ScheduleBlock sBlock = scheduleManager.AddBlock(state.category, state.room, time, length, jobColor, assignedPlayers);
            scheduleManager.AddBlock(state.category, state.room, time, length, jobColor, new List<PlayerRef>() { player });
            taskBlocks.Add(state, sBlock);
        }
    }

    private void DeployTaskBlock(TaskState state, float time, float length, List<PlayerRef> players)
    {
        Debug.Log("Task block deployed, players: " + players.Count);
        if (!taskBlocks.ContainsKey(state))
        {
            Debug.Log("Adding new state");
            ScheduleBlock sBlock = scheduleManager.AddBlock(state.category, state.room, time, length, jobColor, players);
            taskBlocks.Add(state, sBlock);
        }
        else
        {
            Debug.Log("State exists");
            List<PlayerRef> assignedPlayers = taskBlocks[state].assignedPlayers;
            assignedPlayers.AddRange(players);
            scheduleManager.RemoveBlock(taskBlocks[state]);
            ScheduleBlock sBlock = scheduleManager.AddBlock(state.category, state.room, time, length, jobColor, assignedPlayers);
            scheduleManager.AddBlock(state.category, state.room, time, length, jobColor, players);
            taskBlocks.Add(state, sBlock);
        }
    }

    private void OnPlayerLeave(PlayerRef player)
    {
        FirePlayer(player);
    }

    /// <summary>
    /// Called when a player is fired
    /// </summary>
    /// <param name="player"></param>
    private void RemovePlayerFromDeployed(PlayerRef player)
    {
        List<TaskState> removedStates = new List<TaskState>();
        foreach (KeyValuePair<TaskState, ScheduleBlock> kvp in taskBlocks)
        {
            Debug.Log("brh");
            List<PlayerRef> assignedPlayers = new List<PlayerRef>(kvp.Value.assignedPlayers);
            Debug.Log("assiged players count: " + assignedPlayers.Count);
            bool changeBlock = false;
            
            if (assignedPlayers.Contains(player))
            {
                changeBlock = true;
                assignedPlayers.Remove(player);
            }
            if (assignedPlayers.Count == 0) // If there are no more players assigned to this, remove the block
            {
                Debug.Log("count 0");
                // Deletes the schedule block, removes from deployed
                // If we are in the future or this block is in the present, remove it
                if (gameManager.currentPeriod < kvp.Value.time || scheduleManager.currentMasterBlocks.Contains(kvp.Value))
                {
                    Debug.Log("Removing block");
                    scheduleManager.RemoveBlock(kvp.Value);
                    if (kvp.Key.closed && !kvp.Key.storedState)
                    {
                        states.Remove(kvp.Key);
                        OnClosedStateEnd?.Invoke(kvp.Key); // We ended the closed state
                    }
                }
                removedStates.Add(kvp.Key);
                continue;
            }
            if (changeBlock)
            {
                ScheduleBlock newBlock = scheduleManager.AddBlock(kvp.Value.periodName, kvp.Value.room, kvp.Value.time, kvp.Value.length, kvp.Value.color, assignedPlayers, new List<int>(kvp.Value.interestGroups));
                scheduleManager.RemoveBlock(kvp.Value);
            }
        }

        foreach (TaskState state in removedStates)
        {
            taskBlocks.Remove(state);
        }
    }

    private void OnPeriodEnd(ScheduleBlock block)
    {
        TaskState deployedState = GetDeployedState(block);
        if (deployedState == null) return;
        if (deployedState.closed)
        {
            states.Remove(deployedState);
            taskBlocks.Remove(deployedState);
            OnClosedStateEnd?.Invoke(deployedState);
            return;
        }
        // Re-add tasks that aren't completed
        bool incomplete = RemoveIncompleteTasks(deployedState);
        if (!incomplete) return;
        if (block.assignedPlayers == null) return;
        foreach (PlayerRef player in block.assignedPlayers)
        {
            StrikePlayer(player);
        }
    }

    /// <summary>
    /// Removes incomplete tasks within a state and clears it. Returns true if there was at least 1 incomplete task.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    bool RemoveIncompleteTasks(TaskState state)
    {
        Debug.Log("removing");
        List<Task> incompleteTasks = state.GetIncompleteTasks();
        foreach (Task task in state.tasks) 
        {
            if (activeTasks.ContainsKey(task)) activeTasks.Remove(task);
        }
        if (incompleteTasks.Count > 0)
        {
            AddTasksToStates(incompleteTasks);
        }
        state.tasks = null;
        return (incompleteTasks.Count > 0);
    }

    // Checks if the current block is within our active blocks. If it is, send this information to the assigned
    void CheckActiveBlock(ScheduleBlock block)
    {
        TaskState deployedState = GetDeployedState(block);
        if (deployedState == null) return;
        // Send the info to assigned tearout
        if (deployedState.closed) OnClosedStateStart?.Invoke(deployedState);
        SendStateInfoToPlayers(deployedState, block);
    }

    private void SendStateInfoToPlayers(TaskState state, ScheduleBlock block)
    {
        List<string> taskNames = new List<string>();
        List<bool> taskCompletions = new List<bool>();
        foreach (Task task in state.tasks)
        {
            taskNames.Add(task.name);
            taskCompletions.Add(task.isCompleted);
        }
        List<PlayerRef> assignedPlayers = block.assignedPlayers;
        foreach (PlayerRef player in assignedPlayers)
        {
            // Send the info to that player
            scheduleUI.RPC_SendTearoutInfo(player, block.periodName, block.room, block.time, block.length, taskNames.ToArray(), taskCompletions.ToArray());
        }
        // send this info to scheduleui rpc
    }

    /// <summary>
    /// Gets the first available time in this player's schedule to do this job. Returns -1 if there was not time found
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private float GetFirstAvailableTime(PlayerRef player, float stateLength)
    {
        /**
         * periodLength
         * periodSpacing
         * periodTimePerDay
        **/
        // Only checks if overlapping with jobs blocks, not any other blocks
        int localMaxDay = gameManager.currentDay + maxDays;
        for (int day = gameManager.currentDay; day < localMaxDay; day++) // Iterate over every day and hour in the future
        {
            float periodStart = periodAddRange.x + gameManager.currentDay * 24;
            float periodEnd = (periodAddRange.y - stateLength) + gameManager.currentDay * 24;
            for (float period = periodStart; period < periodEnd; period += 0.25f)
            {
                bool isValid = true;
                float periodTime = period + day * 24f;
                foreach (ScheduleBlock block in scheduleManager.playerSchedules[player])
                {
                    bool doesntFit;
                    if (GetDeployedState(block) != null)
                    {
                        doesntFit = ScheduleManager.TimeOverlaps(periodTime - periodSpacing, periodTime + stateLength + periodSpacing, block.time, block.time + block.length);
                    }
                    else
                    {
                        doesntFit = ScheduleManager.TimeOverlaps(periodTime, periodTime + stateLength, block.time, block.time + block.length);
                    }
                    if (doesntFit)
                    {
                        isValid = false;
                        break; // If this block overlaps with our current period
                    }
                }
                if (isValid) return periodTime;
            }
        }
        return -1f;
    }

    private TaskState GetDeployedState(ScheduleBlock block, bool useEquals = false)
    {
        foreach (KeyValuePair<TaskState, ScheduleBlock> kvp in taskBlocks)
        {
            if (useEquals)
            {
                if (kvp.Value.Equals(block)) return kvp.Key;
                continue;
            }
            if (kvp.Value == block) return kvp.Key;
        }
        return null;
    }

    bool IsStateFull(TaskState state)
    {
        if (state.tasks == null) return true;
        float totalTime = 0f;
        float maxTime = periodLength * gameManager.hourLength;
        foreach (Task task in state.tasks)
        {
            totalTime += task.secondsTaken;
            if (totalTime > maxTime) return true;
        }
        return false;
    }
}
