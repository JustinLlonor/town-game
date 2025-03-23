using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using WebSocketSharp;

public class ConflictManager : NetworkBehaviour
{
    public float conflictLength = 3.4f;
    public List<Conflict> engagements = new List<Conflict>();
    public PlayerManager playerManager;
    public ObjectManager objectManager;

    public delegate void ConflictEvent();
    bool init = false;

    /// <summary>
    /// The class for all attacks
    /// </summary>
    [System.Serializable]
    public class Conflict
    {
        public TickTimer conflictEndTimer;
        public PlayerRef attacker;
        public float attackPower;
        public PlayerRef defender;
        public float defensePower;

        // Conflict real time stuff
        public bool defenderPassed;

        public Conflict(TickTimer conflictEndTimer, PlayerRef attacker, float attackPower, PlayerRef defender, float defensePower)
        {
            this.conflictEndTimer = conflictEndTimer;
            this.attacker = attacker;
            this.attackPower = attackPower;
            this.defender = defender;
            this.defensePower = defensePower;
            this.defenderPassed = false;
        }
    }

    public override void Spawned()
    {
        init = true;
    }

    private void Update()
    {
        CheckConflicts();
    }

    void CheckConflicts()
    {
        if (!init) return;
        if (!Runner.IsServer) return; // Only execute this code for the server
        if (engagements.Count == 0) return;
        // Call the end conflict function and remove the conflict if the timer has expired
        List<Conflict> destroyedConflicts = new List<Conflict>();
        foreach (Conflict conflict in engagements)
        {
            if (conflict.conflictEndTimer.ExpiredOrNotRunning(Runner))
            {
                EndConflict(conflict);
                destroyedConflicts.Add(conflict);
            }
        }
        foreach (Conflict conflict in destroyedConflicts) engagements.Remove(conflict);
    }

    void EndConflict(Conflict conflict)
    {
        if (conflict.defenderPassed)
        {
            ConflictTie(conflict);
        }
        else
        {
            ConflictWin(conflict);
        }
    }

    void ConflictWin(Conflict conflict)
    {
        GameObject aGo = playerManager.GetPlayerObject(conflict.attacker);
        GameObject dGo = playerManager.GetPlayerObject(conflict.defender);

        dGo.GetComponent<PlayerStats>().Kill();

        aGo.GetComponent<PlayerMovement>().Unfreeze();
        AttackManager attackerAM = aGo.GetComponent<AttackManager>();
        attackerAM.isEngaged = false;
        PlayerInventory attackerInventory = aGo.GetComponent<PlayerInventory>();
        attackerInventory.canSwitchSlots = true;
        aGo.GetComponent<Player>().lockedPlayer = PlayerRef.None;
    }

    void ConflictTie(Conflict conflict)
    {
        GameObject aGo = playerManager.GetPlayerObject(conflict.attacker);
        GameObject dGo = playerManager.GetPlayerObject(conflict.defender);

        aGo.GetComponent<PlayerMovement>().Unfreeze();
        dGo.GetComponent<PlayerMovement>().Unfreeze();

        AttackManager attackerAM = aGo.GetComponent<AttackManager>();
        AttackManager defenderAM = dGo.GetComponent<AttackManager>();
        attackerAM.isEngaged = false;
        defenderAM.isEngaged = false;

        PlayerInventory attackerInventory = aGo.GetComponent<PlayerInventory>();
        PlayerInventory victimInventory = dGo.GetComponent<PlayerInventory>();
        attackerInventory.canSwitchSlots = true;
        victimInventory.canSwitchSlots = true;

        aGo.GetComponent<Player>().lockedPlayer = PlayerRef.None;
        dGo.GetComponent<Player>().lockedPlayer = PlayerRef.None;
    }

    /// <summary>
    /// Starts an engagement for 2 players
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    public void StartEngagement(PlayerRef attacker, PlayerRef defender, Weapon attackWeapon)
    {
        if (PlayerIsFighting(attacker) || PlayerIsFighting(defender)) return;

        GameObject aGo = playerManager.GetPlayerObject(attacker);
        GameObject dGo = playerManager.GetPlayerObject(defender);

        // Variable setting for conflict
        aGo.GetComponent<PlayerMovement>().Freeze();
        dGo.GetComponent<PlayerMovement>().Freeze();

        AttackManager attackerAM = aGo.GetComponent<AttackManager>();
        AttackManager defenderAM = dGo.GetComponent<AttackManager>();
        attackerAM.isEngaged = true;
        defenderAM.isEngaged = true;

        PlayerInventory attackerInventory = aGo.GetComponent<PlayerInventory>();
        PlayerInventory victimInventory = dGo.GetComponent<PlayerInventory>();
        attackerInventory.canSwitchSlots = false;
        victimInventory.canSwitchSlots = false;

        // Create thge conflict
        TickTimer conflictTimer = TickTimer.CreateFromSeconds(Runner, conflictLength);
        // Get attacker's weapon power
        int attackPower = attackWeapon.strength;
        int defensePower = 0;
        int defenseSlot = GetStrongestDefenseWeapon(victimInventory.hotbar);
        if (defenseSlot >= 0)
        {
            victimInventory.EquipItem(defenseSlot);
            Weapon foundWeapon = (Weapon)objectManager.itemSearch[victimInventory.hotbar[defenseSlot].ToString()]; // The weapon found in the slot defenseSlot of the victim's inventory.
            defensePower = foundWeapon.defense;
        }

        // Add this engagement to the list
        engagements.Add(new Conflict(conflictTimer, attacker, attackPower, defender, defensePower));

        // Start camera lerp
        Player attackPlayer = aGo.GetComponent<Player>();
        Player defenderPlayer = dGo.GetComponent<Player>();
        attackPlayer.RPC_ResetInput();
        defenderPlayer.RPC_ResetInput();
        attackPlayer.lockedPlayer = defender;
        defenderPlayer.lockedPlayer = attacker;

        // Play engagement sequence on player end
        attackerAM.RPC_StartEngagementSequence(attacker, true, attackPower, defensePower);
        defenderAM.RPC_StartEngagementSequence(defender, false, attackPower, defensePower);
    }

    /// <summary>
    /// Returns true if the player is currently in an altercation. Checks all conflicts to see if the player is in one.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool PlayerIsFighting(PlayerRef player)
    {
        foreach (Conflict conflict in engagements)
        {
            if (conflict.attacker == player || conflict.defender == player) return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the conflict from the active conflicts in which this player is a victim. Returns null if nothing was found
    /// </summary>
    /// <param name="victim"></param>
    /// <returns></returns>
    public Conflict GetConflictFromVictim(PlayerRef victim)
    {
        foreach (Conflict conflict in engagements)
        {
            Debug.Log("engagement 1:");
            Debug.Log(conflict.defender);
            Debug.Log(victim);
            Debug.Log(victim == conflict.defender);
            Debug.Log(victim.Equals(conflict.defender));
            if (conflict.defender.Equals(victim)) return conflict;
        }
        return null;
    }

    /// <summary>
    /// Gets the strongest weapon for defense with a given hotbar. Returns -1 if there are no weapons.
    /// </summary>
    /// <returns>The weapon index of the highest defense weapon</returns>
    int GetStrongestDefenseWeapon(NetworkLinkedList<NetworkString<_32>> hotbar)
    {
        int i = -1;
        int highestDefense = -1;
        int highestDefenseWeapon = -1;
        foreach (NetworkString<_32> itemString in hotbar)
        {
            i++;
            if (itemString.ToString().IsNullOrEmpty()) continue; // Empty slot, continue
            Item item = objectManager.itemSearch[itemString.ToString()];
            if (!(item is Weapon)) continue; // If the item is not a weapon, not applicable.
            Weapon weapon = (Weapon)item;
            if (weapon.defense > highestDefense) // Greater than highest defense, set highest defense weapon to this index.
            {
                highestDefense = weapon.defense;
                highestDefense = i;
            }
        }
        return highestDefenseWeapon;
    }

    /// <summary>
    /// Called when the victim of a quicktime event has won their quicktime event.
    /// </summary>
    public void WonQuicktime(PlayerRef wonPlayer)
    {
        Conflict victimConflict = GetConflictFromVictim(wonPlayer);
        Debug.Log("1a");
        if (victimConflict == null) return; // Return if they are not in a conflict as a victim
        Debug.Log("2a");
        if (victimConflict.defensePower == 0) return; // The player cannot defend by definition, return
        Debug.Log("3a");
        victimConflict.defenderPassed = true;
    }
}
