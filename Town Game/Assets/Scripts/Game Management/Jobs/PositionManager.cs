using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;

public class PositionManager : NetworkBehaviour
{
    [Header("If jobs exceed 10 in a branch, change the job property code.")]
    public Branch[] branches;
    public BranchEvent OnLeaderAdd;
    public BranchEvent OnLeaderRemove;
    [Networked, Capacity(20)] NetworkDictionary<PlayerRef, NetworkString<_64>> playerJobs => default;
    [Networked, Capacity(20)] NetworkDictionary<PlayerRef, int> playerBranches => default;
    public JobEvent onJobAdd;
    public JobEvent onJobRemove;
    public IntEvent onBranchSwitch;
    List<Vector2Int> previousJobs = new List<Vector2Int>();
    int previousBranch = -1;
    RunnerManager runnerManager;
    PlayerManager playerManager;
    PlayerRef trackedPlayer;
    bool init = false;

    public delegate void BranchEvent(PlayerRef player, string branch);
    public delegate void PlayerEvent(PlayerRef player);
    public delegate void JobEvent(Vector2Int jobRef);
    public delegate void IntEvent(int integer);

    public static PositionManager i;

    private void Awake()
    {
        i = this;
    }

    public override void Spawned()
    {
        init = true;
        runnerManager = FindFirstObjectByType<RunnerManager>();
        playerManager = FindFirstObjectByType<PlayerManager>();
        if (!Runner.IsServer) return;
        runnerManager.onPlayerLeave += PlayerLeave;
    }

    private void FixedUpdate()
    {
        if (!init) return;
        trackedPlayer = runnerManager.nRunner.LocalPlayer;
        if (trackedPlayer.Equals(PlayerRef.None)) return;
        CheckPositionChange();
        CheckBranchChange();
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

    public void AddPlayerToBranch(PlayerRef player, int branchIndex)
    {
        Branch branch = GetBranchFromIndex(branchIndex);
        if (branch == null) return;
        AddPlayerToBranch(player, branch);
    }

    public void AddPlayerToBranch(PlayerRef player, Branch branch)
    {
        int branchIndex = Array.IndexOf(branches, branch);
        int playerBranch = GetBranch(player);
        if (branchIndex == playerBranch) return;
        Branch previousBranch = GetBranchFromIndex(playerBranch);
        if (previousBranch != null) RemovePlayerFromBranch(player, previousBranch);
        branch.players.Add(player);
        SetBranchProperty(player, branchIndex);
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
    public Vector2Int GetJobHandlerFromRef(JobHandler handler)
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
        if (index >= branches.Length || index < 0) return null;
        return branches[index];
    }

    // Job property stuff

    /// <summary>
    /// Stores the job ref within PositionManager
    /// </summary>
    /// <param name="player"></param>
    /// <param name="jobRef"></param>
    public void AddJobProperty(PlayerRef player, Vector2Int jobRef)
    {
        string jobString = jobRef.x.ToString() + jobRef.y.ToString();
        if (!playerJobs.ContainsKey(player))
        {
            playerJobs.Add(player, jobString);
            return;
        }
        string newJobString = playerJobs[player].ToString();
        newJobString += jobString;
        playerJobs.Set(player, newJobString);
    }

    /// <summary>
    /// Removes the job ref from from this player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="checkedJob"></param>
    public void RemoveJobProperty(PlayerRef player, Vector2Int checkedJob)
    {
        if (!playerJobs.ContainsKey(player)) return;
        string jobString = playerJobs[player].ToString();
        for (int i = 0; i < jobString.Length; i += 2)
        {
            int branchRef = jobString[i] - '0';
            int jobRef = jobString[i + 1] - '0';
            Vector2Int iJob = new Vector2Int(branchRef, jobRef);
            if (checkedJob.Equals(iJob))
            {
                jobString = jobString.Remove(i, 2);
                break;
            }
        }
        playerJobs.Set(player, jobString);
    }

    /// <summary>
    /// Gets the job count for this player
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public int GetJobCount(PlayerRef player)
    {
        if (!playerJobs.ContainsKey(player)) return 0;
        return playerJobs[player].ToString().Length / 2;
    }

    /// <summary>
    /// Returns a list of job refs associated with this player
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public Vector2Int[] GetJobRefs(PlayerRef player)
    {
        if (!playerJobs.ContainsKey(player)) return new Vector2Int[0];
        List<Vector2Int> output = new List<Vector2Int>();
        string jobString = playerJobs[player].ToString();
        for (int i = 0; i < jobString.Length; i += 2)
        {
            int branchRef = jobString[i] - '0';
            int jobRef = jobString[i + 1] - '0';
            output.Add(new Vector2Int(branchRef, jobRef));
        }
        return output.ToArray();
    }

    /// <summary>
    /// Checks if a player has a certain job
    /// </summary>
    /// <param name="player"></param>
    /// <param name="checkedJob"></param>
    /// <returns></returns>
    public bool PlayerHasJob(PlayerRef player, Vector2Int checkedJob)
    {
        if (checkedJob.y < 0)
        {
            return GetBranch(player) == checkedJob.x;
        }
        if (!playerJobs.ContainsKey(player)) return false;
        string jobString = playerJobs[player].ToString();
        for (int i = 0; i < jobString.Length; i += 2)
        {
            int branchRef = jobString[i] - '0';
            int jobRef = jobString[i + 1] - '0';
            Vector2Int iJob = new Vector2Int(branchRef, jobRef);
            if (checkedJob.Equals(iJob)) return true;
        }
        return false;
    }

    public void SetBranchProperty(PlayerRef player, int branchIndex)
    {
        playerBranches.Set(player, branchIndex);
    }

    /// <summary>
    /// Gets the branch index of the specified player
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public int GetBranch(PlayerRef player)
    {
        if (!playerBranches.ContainsKey(player)) return -1;
        return playerBranches[player];
    }

    /// <summary>
    /// Gets the number of players hired for a certain job
    /// </summary>
    /// <param name="jobRef"></param>
    /// <returns></returns>
    public int GetJobPlayerCount(Vector2Int jobRef)
    {
        if (jobRef.y < 0) return -1;
        int count = 0;
        foreach (KeyValuePair<PlayerRef, NetworkString <_64>> kvp in playerJobs)
        {
            if (PlayerHasJob(kvp.Key, jobRef)) count++;
        }
        return count;
    }

    public int GetBranchPlayerCount(int branch)
    {
        int count = 0;
        foreach (KeyValuePair<PlayerRef, int> kvp in playerBranches)
        {
            if (kvp.Value == branch) count++;
        }
        return count;
    }

    private void CheckPositionChange()
    {
        // Add check
        Vector2Int[] currentJobs = GetJobRefs(trackedPlayer);
        foreach (Vector2Int job in currentJobs)
        {
            if (!previousJobs.Contains(job))
            {
                previousJobs.Add(job);
                onJobAdd?.Invoke(job);
            }
        }
        // Remove check
        List<Vector2Int> removalList = new List<Vector2Int>();
        foreach (Vector2Int job in previousJobs)
        {
            if (!currentJobs.Contains(job))
            {
                removalList.Add(job);
                onJobRemove?.Invoke(job);
            }
        }
        foreach (Vector2Int job in removalList)
        {
            previousJobs.Remove(job);
        }
    }

    private void CheckBranchChange()
    {
        int currentBranch = GetBranch(trackedPlayer);
        if (currentBranch != previousBranch)
        {
            previousBranch = currentBranch;
            onBranchSwitch?.Invoke(currentBranch);
        }
    }

    /// <summary>
    /// Returns true if a player has access to the specified room
    /// </summary>
    /// <param name="player"></param>
    /// <param name="room"></param>
    /// <returns></returns>
    public bool PlayerHasAccessToRoom(PlayerRef player, string room)
    {
        Vector2Int[] jobRefs = GetJobRefs(player);
        foreach (Vector2Int jobRef in jobRefs)
        {
            Job job = GetJobFromRef(jobRef);
            if (job.buildingAccess.Contains(room)) return true;
        }
        return false;
    }
}
