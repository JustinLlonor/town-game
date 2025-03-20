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
        public ConflictEvent onConflictCancel;

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
        // call end conflict delegate
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
        aGo.GetComponent<Player>().lockedPlayer = defender;
        dGo.GetComponent<Player>().lockedPlayer = attacker;

        // Tell the players to play the FPS animation on their end
        attackerAM.RPC_PlayAttackAnimation(attacker);
        defenderAM.RPC_PlayDefenseAnimation(defender);
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
}
