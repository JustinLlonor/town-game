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
        if (index >= branches.Length) return null;
        return branches[index];
    }
}
