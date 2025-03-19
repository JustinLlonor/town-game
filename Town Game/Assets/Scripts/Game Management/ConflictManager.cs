using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ConflictManager : NetworkBehaviour
{
    public List<Conflict> engagements = new List<Conflict>();
    public PlayerManager playerManager;
    public float engagementLength;

    public delegate void ConflictEvent();

    /// <summary>
    /// The class for all attacks
    /// </summary>
    [System.Serializable]
    public class Conflict
    {
        public PlayerRef attacker;
        public float attackPower;
        public PlayerRef defender;
        public float defensePower;
        public ConflictEvent onConflictCancel;

        public Conflict(PlayerRef attacker, float attackPower, PlayerRef defender, float defensePower, ConflictEvent onConflictCancel)
        {
            this.attacker = attacker;
            this.attackPower = attackPower;
            this.defender = defender;
            this.defensePower = defensePower;
            this.onConflictCancel = onConflictCancel;
        }
    }

    /// <summary>
    /// Starts an engagement for 2 players
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    public void StartEngagement(PlayerRef attacker, PlayerRef defender)
    {
        // Set players to engaged
        Debug.LogWarning("Started a conflict!");

        GameObject aGo = playerManager.GetPlayerObject(attacker);
        GameObject dGo = playerManager.GetPlayerObject(defender);

        aGo.GetComponent<PlayerMovement>().Freeze();
        dGo.GetComponent<PlayerMovement>().Freeze();
    }
}
