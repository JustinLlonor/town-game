using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class JobManager : NetworkBehaviour
{
    public JobBranch[] branches;
    /// <summary>
    /// The branches that each player is in
    /// </summary>
    [Networked, Capacity(20)] NetworkDictionary<PlayerRef, int> playerBranches => default;
    /// <summary>
    /// The status of each player in each branch, lower number means a higher position
    /// </summary>
    [Networked, Capacity(20)] NetworkDictionary<PlayerRef, int> playerPositions => default;
    /// <summary>
    /// The performance status of each player. Players with higher performance will be promoted to the next level over players of lower performance.
    /// Players will be ordered in the tab menu according to performance
    /// </summary>
    [Networked, Capacity(20)] NetworkDictionary<PlayerRef, int> playerPerformance => default;

    /// <summary>
    /// Gets all the players in a branch
    /// </summary>
    /// <param name="branch"></param>
    /// <returns></returns>
    public List<PlayerRef> GetAllPlayersFromBranch(int branch)
    {
        List<PlayerRef> output = new List<PlayerRef>();
        foreach (KeyValuePair<PlayerRef, int> kvp in playerBranches)
        {
            if (branch == kvp.Value) output.Add(kvp.Key);
        }
        return output;
    }

    /// <summary>
    /// Gets the player's branch
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public int GetBranch(PlayerRef player)
    {
        if (!playerBranches.ContainsKey(player)) return -1;
        return playerBranches[player];
    }

    /// <summary>
    /// Sets the branch of the player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="branch"></param>
    public void SetBranch(PlayerRef player, int branch)
    {
        if (!playerBranches.ContainsKey(player))
        {
            playerBranches.Add(player, branch);
            return;
        }
        playerBranches.Set(player, branch);
    }

    /// <summary>
    /// Removes the player from their current job branch and from the JobManager    
    /// </summary>
    /// <param name="player"></param>
    public void RemovePlayer(PlayerRef player)
    {
        if (playerBranches.ContainsKey(player)) playerBranches.Remove(player);
    }

    public int GetPosition(PlayerRef player)
    {
        if (!playerPositions.ContainsKey(player)) return -1;
        return playerPositions[player];
    }

    public void SetPosition(PlayerRef player, int position)
    {
        if (!playerPositions.ContainsKey(player))
        {
            playerPositions.Add(player, position);
            return;
        }
        playerPositions.Set(player, position);
    }

    public int GetPerformance(PlayerRef player)
    {
        if (!playerPerformance.ContainsKey(player)) return 0;
        return playerPerformance[player];
    }

    public void SetPerformance(PlayerRef player, int performance)
    {
        if (!playerPerformance.ContainsKey(player))
        {
            playerPerformance.Add(player, performance);
            return;
        }
        playerPositions.Set(player, performance);
    }

    /// <summary>
    /// Updates the positions within a branch
    /// </summary>
    /// <param name="branch"></param>
    public void UpdatePositions(int branch)
    {
        JobBranch targetBranch = branches[branch];
        // Get the players in the branch
        List<PlayerRef> players = GetAllPlayersFromBranch(branch);
        // Initialize position counts
        int[] positionCounts = new int[targetBranch.positionLimits.Length];
        foreach (PlayerRef player in players)
        {
            positionCounts[GetPosition(player)]++;
        }

        // Check if there is space to promote
        for (int i = positionCounts.Length-2; i >= 0; i--)
        {
            // How many positions can be promoted to this level
            int newPositions = targetBranch.positionLimits[i] - positionCounts[i];
            while (newPositions > 0)
            {
                // Get the highest performer in the position below
                PlayerRef highestPerformer = PlayerRef.None;
                int highestScore = -1;
                foreach (PlayerRef player in players)
                {
                    // continue if not in the level above
                    if (GetPosition(player) != i + 1) continue;
                    int currentPerformance = GetPerformance(player);
                    if (currentPerformance > highestScore)
                    {
                        highestPerformer = player;
                        highestScore = currentPerformance;
                    }
                }
                // Break if there is no one else in the previous level
                if (highestPerformer == PlayerRef.None) break;
                PromotePlayer(highestPerformer, i);
                newPositions--;
            }
        }

    }

    private void PromotePlayer(PlayerRef player, int newPosition)
    {
        SetPerformance(player, 0);
        SetPosition(player, newPosition);
    }

    public void DemotePlayer(PlayerRef player)
    {

    }
}
