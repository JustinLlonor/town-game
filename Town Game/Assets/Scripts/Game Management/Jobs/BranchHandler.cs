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
public class BranchHandler : MonoBehaviour
{
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

    public Assignable[] branchTasks;
    /// <summary>
    /// The active tasks in this branch
    /// Key = branch id from branch tasks
    /// Value = bitmask of players who are assigned to this task (game ids)
    /// </summary>
    [Networked, Capacity(16)]
    public NetworkDictionary<NetworkString<_8>, int> activeTasks => default;
    /// <summary>
    /// The set deadlines of each task. Not all tasks have deadlines
    /// </summary>
    [Networked, Capacity(16)]
    public NetworkDictionary<NetworkString<_8>, float> deadlines => default;
    [Networked, Capacity(16)]
    public NetworkDictionary<NetworkString<_8>, int> subtaskStage => default;
    /// <summary>
    /// # of tasks assigned to each player
    /// </summary>
    [Networked, Capacity(15)]
    public NetworkDictionary<PlayerRef, int> taskCounts => default;
    public int branch;
    public BranchManager branchManager;


    /// <summary>
    /// Activates a task to be automatically assigned to players
    /// </summary>
    /// <param name="id"></param>
    /// <returns>False if the task is already activated</returns>
    public bool ActivateTask(string id)
    {
        if (activeTasks.ContainsKey(id)) return false;
        activeTasks.Add(id, 0);
        UpdateAssignment(id);
        return true;
    }

    /// <summary>
    /// Activates a task with a deadline to be automatically assigned to players
    /// </summary>
    /// <param name="id"></param>
    /// <returns>False if the task is already activated</returns>
    public bool ActivateTask(string id, float deadline)
    {
        if (activeTasks.ContainsKey(id)) return false;
        activeTasks.Add(id, 0);
        deadlines.Add(id, deadline);
        UpdateAssignment(id);
        return true;
    }

    /// <summary>
    /// Deactivates a task without any awards/punishments
    /// </summary>
    /// <param name="id"></param>
    /// <returns>False if the task is already inactive</returns>
    public bool DeactivateTask(string id)
    {
        if (!activeTasks.ContainsKey(id)) return false;
        activeTasks.Remove(id);
        if (deadlines.ContainsKey(id)) deadlines.Remove(id);
        return true;
    }

    /// <summary>
    /// Deactivates a task while rewarding or punishing the player
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool CompleteTask(string id)
    {
        bool deactivated = DeactivateTask(id);
        if (deactivated)
        {
            // Code for rewarding/punishing players based on deadline
        }
        return deactivated;
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
            branchManager.GetPosition(b) + (1f - (GetTaskCount(b) / 100f))
            .CompareTo(
            branchManager.GetPosition(a) + (1f - (GetTaskCount(b) / 100f))
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
            toAssign--;
        }
    }

    /// <summary>
    /// Clears all tasks for the specified player
    /// </summary>
    /// <param name="player"></param>
    public void ClearAssignment(PlayerRef player)
    {
        // iterate over every task, if player is assigned to it unassign them
        int gameId = PlayerManager.i.GetGameId(player);
        foreach (KeyValuePair<NetworkString<_8>, int> kvp in activeTasks)
        {
            int bitmask = kvp.Value;
            if (GetBit(bitmask, gameId)) UnassignPlayer((string)kvp.Key, player);
            UpdateAssignment((string)kvp.Key); // Update the assignment after the player is done
        }
    }

    private void AssignPlayer(string id, PlayerRef player)
    {
        if (!activeTasks.ContainsKey(id)) return;
        int gameId = PlayerManager.i.GetGameId(player);
        int taskBitmask = activeTasks.Get(id);
        if (GetBit(taskBitmask, gameId)) return;
        // Set the bit corresponding to the player's game id to true
        activeTasks.Set(id, SetBit(taskBitmask, gameId, true));
        // Increase task count
        IncreaseTaskCount(player);
    }

    private void UnassignPlayer(string id, PlayerRef player)
    {
        if (!activeTasks.ContainsKey(id)) return;
        int gameId = PlayerManager.i.GetGameId(player);
        int taskBitmask = activeTasks.Get(id);
        if (!GetBit(taskBitmask, gameId)) return;
        // Set the bit corresponding to the player's game id to false
        activeTasks.Set(id, SetBit(taskBitmask, gameId, false));
        // decrease task count
        DecreaseTaskCount(player);
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

    private void ClearTaskCount(PlayerRef player)
    {
        if (!taskCounts.ContainsKey(player)) return;
        taskCounts.Remove(player);
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
}
