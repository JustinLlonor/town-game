using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class PositionManager : NetworkBehaviour
{
    public Branch[] branches;
    public BranchEvent OnLeaderAdd;
    public BranchEvent OnLeaderRemove;
    RunnerManager runnerManager;
    PlayerManager playerManager;

    public delegate void BranchEvent(PlayerRef player, string branch);

    [System.Serializable]
    public class Branch
    {
        [Header("Static info")]
        public string name;
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

    public override void Spawned()
    {
        runnerManager = FindFirstObjectByType<RunnerManager>();
        playerManager = FindFirstObjectByType<PlayerManager>();
        if (!Runner.IsServer) return;
        runnerManager.onPlayerLeave += PlayerLeave;
    }

    void PlayerLeave(PlayerRef player)
    {
        RemovePlayerFromAllBranches(player);
    }

    /// <summary>
    /// Adds a player to a branch
    /// </summary>
    /// <param name="player"></param>
    /// <param name="branch"></param>
    public void AddPlayerToBranch(PlayerRef player, string branchName)
    {
        Branch branch = GetBranch(branchName);
        if (branch == null) return;
        AddPlayerToBranch(player, branch);
    }

    public void AddPlayerToBranch(PlayerRef player, Branch branch)
    {
        int branchIndex = Array.IndexOf(branches, branch);
        int playerBranch = playerManager.playerProperties[player].branch;
        if (branchIndex == playerBranch) return;
        Branch previousBranch = GetBranchFromIndex(playerBranch);
        if (previousBranch == null) return;
        RemovePlayerFromBranch(player, previousBranch);
        branch.players.Add(player);
    }

    public void RemovePlayerFromBranch(PlayerRef player, string branchName)
    {
        Branch branch = GetBranch(branchName);
        if (branch == null) return;
        RemovePlayerFromBranch(player, branch);
    }

    public void RemovePlayerFromBranch(PlayerRef player, Branch branch)
    {
        if (branch.leader == player)
        {
            RemoveLeader(branch);
        }
        if (branch.leader == player)
        {
            RemoveLeader(branch);
        }
        foreach (Job job in branch.jobs)
        {
            job.RemovePlayer(player);
        }
        if (branch.players.Contains(player))
        {
            branch.players.Remove(player);
        }
    }

    public void RemovePlayerFromAllBranches(PlayerRef player)
    {
        foreach (Branch branch in branches)
        {
            RemovePlayerFromBranch(player, branch);
        }
    }

    public void RemoveLeader(string branchName)
    {
        Branch branch = GetBranch(branchName);
        if (branch == null) return;
        RemoveLeader(branch);
    }

    public void RemoveLeader(Branch branch)
    {
        if (branch.leader == PlayerRef.None) return;
        OnLeaderRemove?.Invoke(branch.leader, branch.name);
        branch.leader = PlayerRef.None;
    }

    /// <summary>
    /// Gets the specified branch with the given name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private Branch GetBranch(string name)
    {
        foreach (Branch branch in branches)
        {
            if (branch.name == name) return branch;
        }
        return null;
    }

    /// <summary>
    /// Gets the vector 2 job reference from the position manager
    /// </summary>
    /// <param name="handler"></param>
    /// <returns>The job reference. The x coordinate is the branch index, and the y coordinate is the job index</returns>
    public Vector2Int GetJobReference(JobHandler handler)
    {
        int branchRef = 0;
        foreach (Branch branch in branches)
        {
            int jobRef = 0;
            foreach (Job job in branch.jobs)
            {
                if (job.handler == handler) return new Vector2Int(branchRef, jobRef);
                jobRef++;
            }
            branchRef++;
        }
        Debug.LogError("Job reference not found!");
        return new Vector2Int(-1, -1);
    }

    /// <summary>
    /// Gets the job object from the job reference
    /// </summary>
    /// <param name="jobRef"></param>
    /// <returns></returns>
    public Job GetJobFromRef(Vector2Int jobRef)
    {
        if (jobRef.x == -1 || jobRef.y == -1) return null;
        if (jobRef.x >= branches.Length) return null;
        Branch branch = branches[jobRef.x];
        if (jobRef.y >= branch.jobs.Length) return null;
        return branch.jobs[jobRef.y];
    }

    public Branch GetBranchFromIndex(int index)
    {
        if (index >= branches.Length) return null;
        return branches[index];
    }
}
