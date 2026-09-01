using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Numerics;
using Photon.Realtime;

/// <summary>
/// Manages tasks and subtasks for a branch
/// </summary>
public class TaskHandler : NetworkBehaviour
{
    /// <summary>
    /// The number of tasks that can be assigned to a position below until it is assigned to a position above
    /// </summary>
    private static float tasksUntilNextLevel = 2f;
    /// <summary>
    /// The maximum amount of money that can be subtracted from a task if it is finished late
    /// </summary>
    private static float maxMoneyReduction = 0.5f;
    /// <summary>
    /// How long after a task deadline until the money punishment reaches max money reduction
    /// </summary>
    private static float moneyPunishLength = 3f;

    public Assignable[] branchTasks;
    /// <summary>
    /// The active tasks in this branch
    /// Key = branch id from branch tasks
    /// Value = bitmask of players who are assigned to this task (game ids)
    /// </summary>
    [Networked, Capacity(16)]
    public NetworkDictionary<NetworkString<_8>, int> activeTasks => default;
    private Dictionary<string, List<Player>> taskPlayerObjects;
    /// <summary>
    /// The set deadlines (period) of each task. Not all tasks have deadlines
    /// </summary>
    [Networked, Capacity(16)]
    public NetworkDictionary<NetworkString<_8>, float> deadlines => default;
    [Networked, Capacity(16)]
    public NetworkDictionary<NetworkString<_8>, int> subtaskStages => default;
    [Networked, Capacity(16)]
    public NetworkDictionary<NetworkString<_8>, int> moneyRewards => default;
    /// <summary>
    /// # of tasks assigned to each player
    /// </summary>
    [Networked, Capacity(15)]
    public NetworkDictionary<PlayerRef, int> taskCounts => default;
    public int branch;
    public BranchManager branchManager;

    public TaskEvent onAssignTask;
    public TaskEvent onUnassignTask;
    public CompletionEvent onCompleteTask;

    /// <summary>
    /// The struct for an assignable task
    /// </summary>
    [System.Serializable]
    public struct Assignable
    {
        [Tooltip("The ID of this task, should be 8 characters or less")]
        public string id;
        public DynamicTask task;
    }

    public delegate void TaskEvent(PlayerRef player, string task);
    public delegate void CompletionEvent(PlayerRef player, CompletionInfo info);

    public override void FixedUpdateNetwork()
    {
        CheckSubtasks();
    }

    /// <summary>
    /// Updates subtask stages based on subtask completion
    /// </summary>
    private void CheckSubtasks()
    {
        foreach (KeyValuePair<NetworkString<_8>, int> kvp in activeTasks)
        {
            string taskId = (string)kvp.Key;
            DynamicTask task = GetTask(taskId);
            int stage = subtaskStages.Get(kvp.Key);
            if (stage >= task.subtasks.Length) continue; // Don't do subtask checking 
            List<Player> assignedPlayers = GetPlayerObjects(taskId);
            // Iterate over previous subtasks to see if level goes down
            bool stageSet = false; // if we need to go back to a previous subtask
            for (int i = 0; i < stage; i++)
            {
                Subtask subtask = task.subtasks[i];
                if (stage >= subtask.completeUntil || subtask.completeUntil == -1) continue;
                if (SubtaskCompleted(subtask, assignedPlayers)) continue;
                // The stage is valid to be checked, and the subtask is incomplete, so set subtask stage to this
                subtaskStages.Set(taskId, i);
                stageSet = true;
            }
            if (stageSet) continue;
            // Check current subtask to see if level can go up
            if (SubtaskCompleted(task.subtasks[stage], assignedPlayers))
            {
                subtaskStages.Set(taskId, stage + 1);
            }
        }
    }

    private bool SubtaskCompleted(Subtask subtask, List<Player> assignedPlayers)
   {
        switch (subtask.completionMode)
        {
            case Subtask.CheckMode.None:
                return subtask.IsCompleted();
            case Subtask.CheckMode.AtLeastOne:
                foreach (Player player in assignedPlayers)
                {
                    if (subtask.IsCompleted(player)) return true;
                }
                return false;
            case Subtask.CheckMode.AllPlayers:
                foreach (Player player in assignedPlayers)
                {
                    if (!subtask.IsCompleted(player)) return false;
                }
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Activates a task to be automatically assigned to players
    /// </summary>
    /// <param name="id"></param>
    /// <returns>False if the task is already activated</returns>
    public bool ActivateTask(string id, int moneyReward = 100)
    {
        if (activeTasks.ContainsKey(id)) return false;
        activeTasks.Add(id, 0);
        subtaskStages.Add(id, 1);
        moneyRewards.Add(id, moneyReward);
        UpdateAssignment(id);
        return true;
    }

    /// <summary>
    /// Activates a task with a deadline to be automatically assigned to players
    /// </summary>
    /// <param name="id"></param>
    /// <returns>False if the task is already activated</returns>
    public bool ActivateTask(string id, float deadline, int moneyReward = 100)
    {
        if (activeTasks.ContainsKey(id)) return false;
        activeTasks.Add(id, 0);
        subtaskStages.Add(id, 1);
        deadlines.Add(id, deadline);
        moneyRewards.Add(id, moneyReward);
        UpdateAssignment(id);
        return true;
    }

    /// <summary>
    /// Deactivates a task without any awards/punishments
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancelEvent">If true, onTaskComplete will be invoked to notify a cancelled event</param>
    /// <returns>False if the task is already inactive</returns>
    public bool DeactivateTask(string id, bool cancelledEvent = true)
    {
        if (!activeTasks.ContainsKey(id)) return false;
        // Decrease task count
        List<PlayerRef> players = branchManager.GetAllPlayersFromBranch(branch);
        foreach (PlayerRef player in players)
        {
            UnassignPlayer(id, player);
            // notifies the players that the event was cancelled
            if (cancelledEvent) onCompleteTask?.Invoke(player, new CompletionInfo(id, -1, -1f, -1f, true));
        }
        activeTasks.Remove(id);
        subtaskStages.Remove(id);
        moneyRewards.Remove(id);
        if (deadlines.ContainsKey(id)) deadlines.Remove(id);
        return true;
    }

    /// <summary>
    /// Deactivates a task while rewarding or punishing the player
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool CompleteTask(string id, Reward reward = Reward.ScaleWithDeadline)
    {
        if (!activeTasks.ContainsKey(id)) return false;
        float deadline = -1f;
        if (deadlines.ContainsKey(id)) deadline = deadlines.Get(id);
        // Process rewards and punishments for all players with this task
        List<PlayerRef> players = branchManager.GetAllPlayersFromBranch(branch);
        foreach (PlayerRef player in players)
        {
            int gameId = PlayerManager.i.GetGameId(player);
            if (GetBit(activeTasks.Get(id), gameId))
            {
                ProcessReward(player, reward, deadline, moneyRewards.Get(id), id);
            }
        }
        DeactivateTask(id, false);
        return false;
    }

    /// <summary>
    /// Processes the reward for a player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="reward"></param>
    /// <param name="deadline"></param>
    /// <param name="money"></param>
    private void ProcessReward(PlayerRef player, Reward reward, float deadline, float money, string id)
    {
        CompletionInfo completionInfo = new CompletionInfo(id, 0, money, 0f, false);
        switch (reward)
        {
            case Reward.ScaleWithDeadline:
                if (deadline == -1f)
                {
                    ChangePerformance(player, 2);
                    PlayerManager.i.AddMoney(player, money);
                    completionInfo.performanceChange = 2;
                    break;
                }
                // Scale performance and money gain based on time
                float currentPeriod = GameManager.i.currentPeriod;
                if (currentPeriod > deadline)
                {
                    int performanceDecrease = Mathf.Clamp(Mathf.FloorToInt(deadline - currentPeriod), -2, -1);
                    ChangePerformance(player, performanceDecrease);
                    // Scale money punishment based on time
                    // percentage of money that is subtracted
                    float punishScale = (Mathf.Clamp(currentPeriod - deadline, 0f, moneyPunishLength) / moneyPunishLength)
                         * maxMoneyReduction;
                    float finalMoneyReward = money - Mathf.RoundToInt(punishScale * money);
                    PlayerManager.i.AddMoney(player, finalMoneyReward);
                    // update completion info
                    completionInfo.performanceChange = performanceDecrease;
                    completionInfo.moneyChange = finalMoneyReward;
                    completionInfo.punishmentPercentage = punishScale;
                }
                else
                {
                    int performanceIncrease = Mathf.Clamp(Mathf.CeilToInt(deadline - currentPeriod), 1, 2);
                    ChangePerformance(player, performanceIncrease);
                    PlayerManager.i.AddMoney(player, money);

                    completionInfo.performanceChange = performanceIncrease;
                }
                break;
            case Reward.FullReward:
                ChangePerformance(player, 2);
                PlayerManager.i.AddMoney(player, money);
                completionInfo.performanceChange = 2;
                break;
            case Reward.HalfReward:
                PlayerManager.i.AddMoney(player, money);
                break;
            case Reward.Punish:
                ChangePerformance(player, -2);
                PlayerManager.i.AddMoney(player, money - (money * maxMoneyReduction));

                completionInfo.performanceChange = -2;
                completionInfo.moneyChange = money - (money * maxMoneyReduction);
                completionInfo.punishmentPercentage = maxMoneyReduction;
                break;
            default:
                break;
        }
        // invoke task completion
        onCompleteTask?.Invoke(player, completionInfo);
    }

    /// <summary>
    /// Assigns a specified task to as many players as possible
    /// </summary>
    /// <param name="id">ID of the task</param>
    public void UpdateAssignment(string id)
    {
        if (!activeTasks.ContainsKey(id)) return;

        int bitmask = activeTasks.Get(id);
        // Get how many new players we can assign
        int assignedPlayers = CountBits(bitmask);
        DynamicTask taskInfo = GetTask(id);
        int maxPlayers = taskInfo.playerLimit;
        int toAssign = maxPlayers - assignedPlayers; // # to assign
        if (toAssign <= 0) return;

        List<PlayerRef> branchPlayers = branchManager.GetAllPlayersFromBranch(branch);
        // Remove players who do not have access to this task
        List<PlayerRef> filteredPlayers = new List<PlayerRef>(branchPlayers);
        int i = filteredPlayers.Count - 1;
        while (i >= 0)
        {
            if (branchManager.GetPosition(filteredPlayers[i]) > taskInfo.level)
            {
                filteredPlayers.RemoveAt(i);
            }
            i--;
        }

        if (filteredPlayers.Count == 0) return;

        // Sort players first by highest to lowest position index, then by lowest to highest task count
        filteredPlayers.Sort((a, b) => 
            branchManager.GetPosition(b) + (0.99f - (GetTaskCount(b) / tasksUntilNextLevel))
            .CompareTo(
            branchManager.GetPosition(a) + (0.99f - (GetTaskCount(b) / tasksUntilNextLevel))
            ));

        // Assign the players while there is still space to assign
        while (toAssign > 0)
        {
            if (filteredPlayers.Count == 0) break;
            // Remove if player already has this task assigned
            if (GetBit(bitmask, PlayerManager.i.GetGameId(filteredPlayers[0])))
            {
                filteredPlayers.RemoveAt(0);
                continue;
            }
            AssignPlayer(id, filteredPlayers[0]);
            filteredPlayers.RemoveAt(0);
            toAssign--;
        }
    }

    /// <summary>
    /// Clears all tasks for the specified player and reassigns them
    /// </summary>
    /// <param name="player"></param>
    public void ClearAssignment(PlayerRef player)
    {
        // iterate over every task, if player is assigned to it unassign them
        int gameId = PlayerManager.i.GetGameId(player);
        List<string> unassignedTasks = new List<string>();
        foreach (KeyValuePair<NetworkString<_8>, int> kvp in activeTasks)
        {
            int bitmask = kvp.Value;
            if (GetBit(bitmask, gameId)) unassignedTasks.Add((string)kvp.Key);
        }
        // Iterate over every task and unassign/reassign them
        foreach (string task in unassignedTasks)
        {
            UnassignPlayer(task, player); // Unassign the player from the task
            UpdateAssignment(task); // Update the assignment after the player is done
        }
    }

    /// <summary>
    /// Assigns a player to a task
    /// </summary>
    /// <param name="id"></param>
    /// <param name="player"></param>
    private void AssignPlayer(string id, PlayerRef player)
    {
        if (!activeTasks.ContainsKey(id)) return;
        int gameId = PlayerManager.i.GetGameId(player);
        int taskBitmask = activeTasks.Get(id);
        if (GetBit(taskBitmask, gameId)) return;
        // Set the bit corresponding to the player's game id to true
        activeTasks.Set(id, SetBit(taskBitmask, gameId, true));
        UpdatePlayerObjects(id);
        // Increase task count
        IncreaseTaskCount(player);
        // invoke the assign task event
        onAssignTask?.Invoke(player, id);
    }

    /// <summary>
    /// Unassigns a player from a task
    /// </summary>
    /// <param name="id"></param>
    /// <param name="player"></param>
    private void UnassignPlayer(string id, PlayerRef player)
    {
        if (!activeTasks.ContainsKey(id)) return;
        int gameId = PlayerManager.i.GetGameId(player);
        int taskBitmask = activeTasks.Get(id);
        if (!GetBit(taskBitmask, gameId)) return;
        // Set the bit corresponding to the player's game id to false
        activeTasks.Set(id, SetBit(taskBitmask, gameId, false));
        UpdatePlayerObjects(id);
        // decrease task count
        DecreaseTaskCount(player);
        // invoke unassign task event
        onUnassignTask?.Invoke(player, id);
    }

    private int GetTaskCount(PlayerRef player)
    {
        if (!taskCounts.ContainsKey(player)) return 0;
        return taskCounts.Get(player);
    }

    private void IncreaseTaskCount(PlayerRef player)
    {
        if (!taskCounts.ContainsKey(player)) taskCounts.Add(player, 0);
        taskCounts.Set(player, taskCounts.Get(player) + 1);
    }

    private void DecreaseTaskCount(PlayerRef player)
    {
        if (!taskCounts.ContainsKey(player)) taskCounts.Add(player, 0);
        int count = taskCounts.Get(player);
        if (count <= 0) return;
        taskCounts.Set(player, count - 1);
    }

    private void ChangePerformance(PlayerRef player, int performanceDelta)
    {
        if (performanceDelta > 0)
        {
            branchManager.AddPerformance(player, performanceDelta);
        } else
        {
            branchManager.RemovePerformance(player, -performanceDelta);
        }
    }

    private void ClearTaskCount(PlayerRef player)
    {
        if (!taskCounts.ContainsKey(player)) return;
        taskCounts.Remove(player);
    }

    /// <summary>
    /// Checks over every task. If this player is assigned to a task that exceeds their position,
    /// they will be unassigned from it.
    /// </summary>
    /// <param name="player"></param>
    public void CheckTasks(PlayerRef player)
    {
        int gameId = PlayerManager.i.GetGameId(player);
        int position = branchManager.GetPosition(player);
        List<string> unassignedTasks = new List<string>();
        foreach (KeyValuePair<NetworkString<_8>, int> kvp in activeTasks)
        {
            if (GetBit(kvp.Value, gameId))
            {
                // If the player's position exceeds the level,
                // they are a lower position than the task and should be unassigned
                DynamicTask taskObj = GetTask((string)kvp.Key);
                if (position > taskObj.level || position == -1)
                {
                    unassignedTasks.Add((string)kvp.Key);
                }
            }
        }

        // Unassign the player from their task and update the assignment
        foreach (string task in unassignedTasks)
        {
            UnassignPlayer(task, player);
            UpdateAssignment(task);
        }
    }

    /// <summary>
    /// Gets the dynamictask object corresponding to the id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public DynamicTask GetTask(string id)
    {
        foreach (Assignable assignable in branchTasks)
        {
            if (assignable.id == id) return assignable.task;
        }
        return null;
    }

    /// <summary>
    /// Updates the player objects to be accessed by the server.
    /// Should be called whenever an active task assignment is updated.
    /// </summary>
    /// <param name="taskId"></param>
    private void UpdatePlayerObjects(string taskId)
    {
        // If task is not active clear and return
        if (!activeTasks.ContainsKey(taskId))
        {
            if (taskPlayerObjects.ContainsKey(taskId)) taskPlayerObjects.Remove(taskId); 
            return;
        }
        // Construct player object list using bitmask
        int bitmask = activeTasks.Get(taskId);
        List<Player> updatedObjects = new List<Player>();
        for (int i = 0; i < 30; i++)
        {
            if (!GetBit(bitmask, i)) continue;
            PlayerRef foundPlayer = PlayerManager.i.GetPlayerFromGameId(i);
            updatedObjects.Add(PlayerManager.i.GetPlayerObject(foundPlayer).GetComponent<Player>());
        }
        // Add to task player objects
        if (!taskPlayerObjects.ContainsKey(taskId))
        {
            taskPlayerObjects.Add(taskId, updatedObjects);
            return;
        }
        taskPlayerObjects[taskId] = updatedObjects;
    }

    private List<Player> GetPlayerObjects(string taskId)
    {
        if (!taskPlayerObjects.ContainsKey(taskId))
        {
            return new List<Player>();
        }
        return taskPlayerObjects[taskId];
    }

    /// <summary>
    /// Helper function for an integer bitmask. Sets the specified bit to 1 or 0
    /// </summary>
    /// <param name="mask">The bitmask we are accessing</param>
    /// <param name="bit">The location of the bit, from 0-31</param>
    /// <param name="value">The new value of the bitmask</param>
    /// <returns>The new bitmask after modfiying</returns>
    private int SetBit(int mask, int bit, bool value)
    {
        if (value)
        {
            // Sets the specified bit to 1
            mask |= 1 << bit;
        }
        else
        {
            // Sets specified bit to 0
            mask &= ~(1 << bit);
        }

        return mask;
    }

    /// <summary>
    /// Helper function to get the specified bit of a bitmask
    /// </summary>
    /// <param name="mask">The bitmask we are accessing</param>
    /// <param name="bit">The location of the bit</param>
    /// <returns>True if the bit is 1, false if 0</returns>
    private bool GetBit(int mask, int bit)
    {
        return (mask & (1 << bit)) != 0;
    }

    /// <summary>
    /// Helper function for getting the number of 1s in a bitmask
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    private int CountBits(int mask)
    {
        int count = 0;

        while (mask != 0)
        {
            mask &= mask - 1;
            count++;
        }

        return count;
    }

    public int GetReward(string task)
    {
        if (!moneyRewards.ContainsKey(task)) return -1;
        return moneyRewards.Get(task);
    }

    public int GetTaskStage(string task)
    {
        if (!subtaskStages.ContainsKey(task)) return -1;
        return subtaskStages.Get(task);
    }
}
