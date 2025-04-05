using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class VotingManager : NetworkBehaviour
{
    public List<VoteInstance> activeVotes = new List<VoteInstance>();
    private GameManager gameManager;

    public delegate void VoteEndEvent(PlayerRef winner);

    [System.Serializable]
    public class VoteInstance
    {
        private static int idCounter = 0;

        /// <summary>
        /// The name of the vote instance, to be announced when a vote commences
        /// </summary>
        public string name;
        /// <summary>
        /// The text that shows when the player hovers over a vote action for this vote instance
        /// </summary>
        public string voteAction;
        /// <summary>
        /// The timer of this vote instance
        /// </summary>
        public float voteTimeEnd;
        /// <summary>
        /// The delegate that gets called when the vote instance ends. The winner PlayerRef may be PlayerRef.None.
        /// </summary>
        public VoteEndEvent onVoteEnd;
        /// <summary>
        /// Every vote given to a player. The key is the voter, the PlayerRef is the voted player.
        /// </summary>
        public Dictionary<PlayerRef, PlayerRef> playerVotes = new Dictionary<PlayerRef, PlayerRef>();
        /// <summary>
        /// The list of players who are allowed to be voted for. If null, then everyone is allowed to be voted for.
        /// </summary>
        public List<PlayerRef> votedWhitelist;
        /// <summary>
        /// The list of players who are allowed to be voters. If null, then everyone is allowed to be a voter.
        /// </summary>
        public List<PlayerRef> voterWhitelist;
        /// <summary>
        /// The id of this vote instance, to be referenced with networking
        /// </summary>
        private int id;

        public VoteInstance(string name, string voteAction, float voteTimeEnd, List<PlayerRef> votedWhitelist, List<PlayerRef> voterWhitelist)
        {
            this.name = name;
            this.voteAction = voteAction;
            this.voteTimeEnd = voteTimeEnd;
            this.votedWhitelist = votedWhitelist;
            this.voterWhitelist = voterWhitelist;
            id = idCounter;
            idCounter++;
        }

        public int GetId() { return id; }

        public void AddVote(PlayerRef voter, PlayerRef voted)
        {
            if (voted == PlayerRef.None)
            if (voter == voted) return;
            if (!PlayerInVotedWhitelist(voted) || !PlayerInVoterWhitelist(voter)) return; // If the voter can vote and if the voted can be voted for
            if (playerVotes.ContainsKey(voter))
            {
                playerVotes[voter] = voted;
                return;
            }
            playerVotes.Add(voter, voted);
        }

        private bool PlayerInVotedWhitelist(PlayerRef voted)
        {
            if (votedWhitelist == null) return true;
            return votedWhitelist.Contains(voted);
        }

        private bool PlayerInVoterWhitelist(PlayerRef voter)
        {
            if (voterWhitelist == null) return true;
            return voterWhitelist.Contains(voter);
        }

        /// <summary>
        /// Gets the player with the most votes. May be randomized if there is a tie.
        /// </summary>
        /// <returns></returns>
        public PlayerRef GetCurrentVoteWinner()
        {
            // Tallies up the vote counts of every player
            Dictionary<PlayerRef, int> voteCounts = new Dictionary<PlayerRef, int>();
            foreach (KeyValuePair<PlayerRef, PlayerRef> votes in playerVotes)
            {
                PlayerRef voted = votes.Value;
                // Add this to the voteCounts dictionary
                if (voteCounts.ContainsKey(voted))
                {
                    voteCounts[voted]++;
                }
                else
                {
                    voteCounts.Add(voted, 1);
                }
            }

            // Get the players with the highest vote counts
            List<PlayerRef> tiedPlayers = new List<PlayerRef>();
            int highestVoteCount = -1;
            foreach (KeyValuePair<PlayerRef, int> count in voteCounts)
            {
                PlayerRef voted = count.Key;
                int voteCount = count.Value;
                if (voteCount > highestVoteCount)
                {
                    highestVoteCount = voteCount;
                    tiedPlayers = new List<PlayerRef>() { voted };
                    continue;
                }
                if (voteCount == highestVoteCount)
                {
                    tiedPlayers.Add(voted);
                }
            }

            // No one voted for anything, return none
            if (highestVoteCount == 0 || highestVoteCount == -1) return PlayerRef.None;

            // Gets a random player in tied players 
            int returnIndex = Random.Range(0, tiedPlayers.Count);
            return tiedPlayers[returnIndex];
        }

        public void EndVote()
        {
            PlayerRef winner = GetCurrentVoteWinner();
            onVoteEnd?.Invoke(winner);
        }
    }

    public override void Spawned()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public override void FixedUpdateNetwork()
    {
        CheckVotes();
    }

    /// <summary>
    /// Checks if any vote instances have ended, and ends them if they have.
    /// </summary>
    private void CheckVotes()
    {
        List<VoteInstance> removedInstances = new List<VoteInstance>();
        foreach (VoteInstance instance in activeVotes)
        {
            // If the timer for voting instance has passed, end the instance
            if (gameManager.gameTime > instance.voteTimeEnd)
            {
                EndVote(instance);
                removedInstances.Add(instance);
            }
        }

        foreach (VoteInstance instance in removedInstances) activeVotes.Remove(instance);
    }

    /// <summary>
    /// Creates a new vote instance with the specified details.
    /// </summary>
    /// <param name="name">The name of the vote instnace</param>
    /// <param name="voteAction">The text taht shows when a player hovers over a vote action for this vote instance</param>
    /// <param name="duration">The time in seconds the vote will take before being terminated</param>
    /// <param name="votedWhitelist">The players who are allowed to be voted for</param>
    /// <param name="voterWhitelist">The players who can participate in this vote</param>
    /// <returns>The vote instance. You can add listener functions to the instance's delegate</returns>
    public VoteInstance StartVote(string name, string voteAction, float duration, List<PlayerRef> votedWhitelist = null, List<PlayerRef> voterWhitelist = null)
    {
        VoteInstance newInstance = new VoteInstance(name, voteAction, gameManager.gameTime + duration, votedWhitelist, voterWhitelist);
        activeVotes.Add(newInstance);
        return newInstance;
    }

    /// <summary>
    /// Removes the VoteInstance from activeVotes
    /// </summary>
    /// <param name="instance"></param>
    public void EndVote(VoteInstance instance)
    {
        if (!activeVotes.Contains(instance)) return;
        instance.EndVote();
        activeVotes.Remove(instance);
    }
}
