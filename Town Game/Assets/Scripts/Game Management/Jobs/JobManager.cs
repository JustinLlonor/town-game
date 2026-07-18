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
    [Networked, Capacity(20)] NetworkDictionary<PlayerRef, int> playerPosition => default;
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

}
