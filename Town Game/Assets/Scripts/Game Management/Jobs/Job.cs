using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[System.Serializable]
public class Job
{
    public JobHandler handler;
    [Header("Info")]
    public string name;
    public string description;
    public string[] buildingAccess = new string[] { };
    public PayLevel pay;
    public TimeLevel timeCommitment;
    public int maxPlayers = 2;
    public List<PlayerRef> assignedPlayers = new List<PlayerRef>();

    public enum PayLevel
    {
        Low = 0,
        Moderate = 1,
        High = 2
    }

    public enum TimeLevel
    {
        Shorter = 0,
        Moderate = 1,
        Longer = 2
    }

    /// <summary>
    /// Adds the player to this job
    /// </summary>
    /// <param name="player"></param>
    public void AddPlayer(PlayerRef player)
    {
        if (assignedPlayers.Contains(player)) return;
        if (assignedPlayers.Count >= maxPlayers) return;
        assignedPlayers.Add(player);
        handler.HirePlayer(player);
    }

    /// <summary>
    /// Removes the player from this job.
    /// </summary>
    /// <param name="player"></param>
    public void RemovePlayer(PlayerRef player)
    {
        if (!assignedPlayers.Contains(player)) return;
        handler.FirePlayer(player);
        assignedPlayers.Remove(player);
    }

    /// <summary>
    /// If the player is hired to this job
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool IsHired(PlayerRef player)
    {
        return assignedPlayers.Contains(player);
    }
}
