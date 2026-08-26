using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Manages branches and players within them
/// </summary>
public class BranchManager : NetworkBehaviour
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
    /// To be called when a player is removed
    /// </summary>
    /// <param name="player"></param>
    private void PlayerRemovalEvent(PlayerRef player)
    {
        RemovePlayer(player);
        // Code for removing from tasks
    }

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
    /// Sets the branch of the player, and resets position and performance
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
        // Set performance to lowest possible
        SetPerformance(player, 0);
        // Set position to lowest possible
        SetPosition(player, branches[branch].GetLowestPosition());
    }

    /// <summary>
    /// Removes the player from their current job branch and from the JobManager    
    /// </summary>
    /// <param name="player"></param>
    public void RemovePlayer(PlayerRef player)
    {
        if (playerBranches.ContainsKey(player))
        {
            int branch = playerBranches.Get(player);
            playerBranches.Remove(player);
            UpdatePositions(branch);
        }
        if (playerPositions.ContainsKey(player)) playerPositions.Remove(player);
        if (playerPerformance.ContainsKey(player)) playerPerformance.Remove(player);
    }

    /// <summary>
    /// Gets a player's position
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public int GetPosition(PlayerRef player)
    {
        if (!playerPositions.ContainsKey(player)) return -1;
        return playerPositions[player];
    }

    /// <summary>
    /// Sets the position of the player and resets the performance
    /// </summary>
    /// <param name="player"></param>
    /// <param name="position"></param>
    public void SetPosition(PlayerRef player, int position)
    {
        if (!playerPositions.ContainsKey(player))
        {
            playerPositions.Add(player, position);
            return;
        }
        playerPositions.Set(player, position);
        SetPerformance(player, 0);
    }

    /// <summary>
    /// Demotes a player and promotes the highest performing player in the level below.
    /// If there is no one in the level below, nothing happens
    /// </summary>
    /// <param name="player"></param>
    public void DemotePlayer(PlayerRef player)
    {
        int branch = GetBranch(player);
        int lowestPos = branches[branch].GetLowestPosition();
        int playerPos = GetPosition(player);
        if (playerPos == lowestPos || playerPos == -1) return;

        // Find the highest performing player in the position below
        List<PlayerRef> players = GetAllPlayersFromBranch(branch);
        PlayerRef highestPerformer = PlayerRef.None;
        int highestScore = int.MinValue;
        foreach (PlayerRef cPlayer in players)
        {
            // continue if not in the level below
            if (GetPosition(cPlayer) != playerPos + 1) continue;
            int currentPerformance = GetPerformance(cPlayer);
            if (currentPerformance > highestScore)
            {
                highestPerformer = cPlayer;
                highestScore = currentPerformance;
            }
        }
        // Return if there is no one else in the level below, the player will not be demoted since there is no one else
        if (highestPerformer == PlayerRef.None) return;

        // Promote highest performer and demote the current player.
        SetPosition(highestPerformer, playerPos);
        SetPosition(player, playerPos + 1);
    }

    /// <summary>
    /// Get the performance score of a player
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public int GetPerformance(PlayerRef player)
    {
        if (!playerPerformance.ContainsKey(player)) return 0;
        return playerPerformance[player];
    }

    /// <summary>
    /// Set the performance score of a player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="performance"></param>
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
    /// Add player performance
    /// </summary>
    /// <param name="player"></param>
    /// <param name="performance"></param>
    public void AddPerformance(PlayerRef player, int performance)
    {
        int newPerformance = GetPerformance(player);
        newPerformance += performance;
        SetPerformance(player, newPerformance);
    }

    /// <summary>
    /// Remove player performance
    /// </summary>
    /// <param name="player"></param>
    /// <param name="performance"></param>
    public void RemovePerformance(PlayerRef player, int performance)
    {
        int newPerformance = GetPerformance(player);
        newPerformance -= performance;
        if (newPerformance < 0) newPerformance = 0;
        SetPerformance(player, newPerformance);
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
        for (int i = positionCounts.Length - 2; i >= 0; i--)
        {
            // How many positions can be promoted to this level
            int newPositions = targetBranch.positionLimits[i] - positionCounts[i];
            while (newPositions > 0)
            {
                // Get the highest performer in the position below
                PlayerRef highestPerformer = PlayerRef.None;
                int highestScore = int.MinValue;
                foreach (PlayerRef player in players)
                {
                    // continue if not in the level below
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
                SetPosition(highestPerformer, i);
                newPositions--;
            }
        }
    }

    /// <summary>
    /// Balances the player counts within each branch so that the distribution is even
    /// </summary>
    public void UpdateBranchCounts()
    {
        int[] branchCounts = GetBranchCounts();
        /**
         * A branch can only transfer players if it has more than 1 player
         * Take the max and minimum branches. If the difference is more than 1, then transfer a player from max to min.
         * Repeat until the difference is <= 1.
        **/

        // Initialize indices of largest/smallest branches
        int maxBranchIndex = -1;
        int minBranchIndex = -1;
        // The difference between the branch with largest and branch with lowest players
        int maxMinDifference = UpdateMaxMinBranches(branchCounts, out maxBranchIndex, out minBranchIndex);
        // Transfer players from max to min as long as the difference is greater than 1
        while (maxMinDifference > 1)
        {
            if (maxBranchIndex == minBranchIndex) break;
            // Transfer players, lowest player in max branch to min branch
            PlayerRef transferredPlayer = GetLowestRankedPlayer(maxBranchIndex);
            if (transferredPlayer == PlayerRef.None)
            {
                Debug.LogError("Player transfer could not be completed");
                return;
            }
            SetBranch(transferredPlayer, minBranchIndex);
            // Update the branch counts for max and min
            branchCounts[maxBranchIndex]--;
            branchCounts[minBranchIndex]++;
            // Update diff
            maxMinDifference = UpdateMaxMinBranches(branchCounts, out maxBranchIndex, out minBranchIndex);
        }
    }

    private int UpdateMaxMinBranches(int[] branchCounts, out int maxBranch, out int minBranch)
    {
        // Initialize max and min branch values
        maxBranch = -1;
        int maxCount = int.MinValue;
        minBranch = -1;
        int minCount = int.MaxValue;
        // Iterate over every branch to find the values
        for (int i = 0; i < branchCounts.Length; i++)
        {
            if (branchCounts[i] > maxCount)
            {
                maxBranch = i; 
                maxCount = branchCounts[i];
            }
            if (branchCounts[i] < minCount)
            {
                minBranch = i; 
                minCount = branchCounts[i];
            }
        }
        // Return the diff
        return maxCount - minCount;
    }

    /// <summary>
    /// Gets an array of the amount of players in each branch. Each index corresponds to its respective branch
    /// </summary>
    /// <returns></returns>
    private int[] GetBranchCounts()
    {
        // Iterate over every player in branches
        int[] branchCounts = new int[branches.Length];
        foreach (KeyValuePair<PlayerRef, int> kvp in playerBranches)
        {
            if (kvp.Value < 0) continue;
            branchCounts[kvp.Value]++;
        }
        return branchCounts;
    }

    /// <summary>
    /// Gets the lowest ranked player in a branch
    /// </summary>
    /// <param name="branch"></param>
    /// <returns></returns>
    private PlayerRef GetLowestRankedPlayer(int branch)
    {
        List<PlayerRef> branchPlayers = GetAllPlayersFromBranch(branch);
        PlayerRef lowestRanked = PlayerRef.None;
        int lowestPerformance = int.MaxValue;
        int lowestPosition = int.MinValue;
        foreach (PlayerRef player in branchPlayers)
        {
            // Get the current player pos and perform
            int position = GetPosition(player);
            int performance = GetPerformance(player);
            // continue if player is ranked higher than lowest ranked
            if (position < lowestPosition) continue;
            // check position first
            if (position > lowestPosition)
            {
                lowestPosition = position;
                lowestRanked = player;
                lowestPerformance = performance;
                continue;
            }
            // check performance if both are equal
            if (performance < lowestPerformance)
            {
                lowestPosition = position;
                lowestRanked = player;
                lowestPerformance = performance;
                continue;
            }
        }
        return lowestRanked;
    }
}
