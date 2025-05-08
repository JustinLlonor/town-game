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
    public Level pay;
    public Level timeCommitment;
    public List<PlayerRef> assignedPlayers = new List<PlayerRef>();

    public enum Level
    {
        Low = 0,
        Moderate = 1,
        High = 2
    }

    /// <summary>
    /// Adds the player to this job
    /// </summary>
    /// <param name="player"></param>
    public void AddPlayer(PlayerRef player)
    {
        if (assignedPlayers.Contains(player)) return;
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
