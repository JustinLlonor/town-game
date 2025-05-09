using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

[System.Serializable]
public class Branch
{
    [Header("Static info")]
    public string name;
    public string description;
    public Texture icon;
    public Color color;
    [Tooltip("The max number of players. If this is set to -1, then this branch has no maximum")]
    public int maxPlayers = -1;
    [Tooltip("The name of the category of rooms involved with this branch")]
    public RoomCategory category;
    [Tooltip("The name of the lead position of this branch ex. Head of Science")]
    public string leadPositionName;
    [Tooltip("The jobs related to this branch")]
    public Job[] jobs;
    [Header("In-game info")]
    public PlayerRef leader;
    public List<PlayerRef> players;

    /// <summary>
    /// Distributes the players among the jobs
    /// </summary>
    public void AssignJobs()
    {
        // Do not assign jobs if there are no players
        if (players.Count == 0) return;
        int jobLimit = GetJobLimit();
        Dictionary<PlayerRef, int> playerJobs = GetPlayerJobCount();
        Job lowestHired = GetLowestHiredJob();
        PlayerRef lowestJob = GetLowestEmployedPlayer(playerJobs);
        // Every job must have at least 1 hired player, and every player must have at most jobLimit jobs
        while (playerJobs[lowestJob] < jobLimit)
        {
            lowestHired.AddPlayer(lowestJob);
        }
    }

    /// <summary>
    /// Gets the job limit
    /// </summary>
    /// <returns></returns>
    private int GetJobLimit()
    {
        return Mathf.CeilToInt((float)jobs.Length / (float)players.Count);
    }

    /// <summary>
    /// Gets a dictionary of players and the amount of jobs they have
    /// </summary>
    /// <returns></returns>
    private Dictionary<PlayerRef, int> GetPlayerJobCount()
    {
        Dictionary<PlayerRef, int> output = new Dictionary<PlayerRef, int>();
        foreach (Job job in jobs)
        {
            foreach (PlayerRef player in job.assignedPlayers)
            {
                if (output.ContainsKey(player))
                {
                    output[player]++;
                }
                else
                {
                    output.Add(player, 1);
                }
            }
        }
        return output;
    }

    private PlayerRef GetLowestEmployedPlayer(Dictionary<PlayerRef, int> playerJobs)
    {
        int lowestJob = 69420;
        PlayerRef lowestPlayer = PlayerRef.None;
        foreach (KeyValuePair<PlayerRef, int> playerJob in playerJobs)
        {
            if (playerJob.Value < lowestJob)
            {
                lowestJob = playerJob.Value;
                lowestPlayer = playerJob.Key;
            }
        }
        return lowestPlayer;
    }

    /// <summary>
    /// Gets the job with the lowest amount of players
    /// </summary>
    /// <returns></returns>
    private Job GetLowestHiredJob()
    {
        int lowestPlayerCount = 5318008;
        Job lowestJob = null;
        foreach (Job job in jobs)
        {
            int playerCount = job.assignedPlayers.Count;
            if (playerCount < lowestPlayerCount)
            {
                lowestPlayerCount = playerCount;
                lowestJob = job;
            }
        }
        return lowestJob;
    }
}
