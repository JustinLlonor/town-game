using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ScheduleManager : NetworkBehaviour
{
    public List<ScheduleBlock> masterSchedule = new List<ScheduleBlock>();
    public List<ScheduleBlock> localSchedule;
    public Dictionary<PlayerRef, List<ScheduleBlock>> playerSchedules = new Dictionary<PlayerRef, List<ScheduleBlock>>(); // Contains the assigned schedule blocks of every player, to be revealed on other clients
    public Dictionary<PlayerRef, List<ScheduleBlock>> proxySchedules = new Dictionary<PlayerRef, List<ScheduleBlock>>(); // For clients, the schedule blocks that are revealed to them
    // Soon to be deprecated code
    #region
    // Dictionary for the schedules of each player
    public Dictionary<PlayerRef, List<ScheduleBlock>> dplayerSchedules => default;
    // this client's schedule
    public List<ScheduleBlock> dlocalSchedule;
    public List<ScheduleBlock> dimmutableBlocks = new List<ScheduleBlock>();
    [HideInInspector] public List<ScheduleBlock> dorderedBlocks = new List<ScheduleBlock>();
    //PhotonView view;
    [HideInInspector] public ScheduleBlock dcurrentBlock = ScheduleBlock.None;
    ScheduleBlock dpreviousBlock = ScheduleBlock.None;
    #endregion

    [HideInInspector] public GameManager gm;

    public ScheduleEvent OnUpdateSchedule;
    public BlockChange OnBlockChange;

    public delegate void ScheduleEvent();
    /// <summary>
    /// Called when a schedule block ends and another begins
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public delegate void BlockChange(ScheduleBlock from, ScheduleBlock to);

    PlayerManager playerManager;

    private void Awake()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
        //view = gameObject.GetComponent<PhotonView>();
        OnUpdateSchedule += UpdateOrderedBlocks;
        gm.OnChangeDay += ResetBlockCheck;
        gm.OnChangeDay += UpdateOrderedBlocks;
        //view.RPC("AddSchedulePlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
    }

    private void Start()
    {
        // Initialize proxy array
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            proxySchedules.Add(player, new List<ScheduleBlock>());
        }
        if (!Runner.IsServer) return;
        // Initialize actual player schedule array
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            playerSchedules.Add(player, new List<ScheduleBlock>());
        }
        //view.RPC("AddScheduleBlock", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, "Patrol", "", 13f, 1f);
    }

    private void Update()
    {
        CheckBlockChange();
    }

    /// <summary>
    /// Adds a schedule block to the master schedule
    /// </summary>
    /// <param name="periodName"></param>
    /// <param name="room"></param>
    /// <param name="time"></param>
    /// <param name="length"></param>
    /// <param name="interestGroups"></param>
    /// <returns>The added schedule block</returns>
    public ScheduleBlock AddBlock(string periodName, string room, float time, float length, List<PlayerRef> assignedPlayers, List<int> interestGroups = null)
    {
        ScheduleBlock newBlock = new ScheduleBlock(periodName, room, length, time, assignedPlayers, interestGroups);
        masterSchedule.Add(newBlock);
        SendAddBlockData(newBlock);
        return newBlock;
    }

    /// <summary>
    /// Removes a schedule block from the master schedule. If the schedule block is in the future, the removal is cancelled.
    /// </summary>
    /// <param name="block"></param>
    public void RemoveBlock(ScheduleBlock block)
    {
        int blockIndex = masterSchedule.IndexOf(block);
        if (blockIndex == -1) return;
        SendRemoveBlockData(block);
        masterSchedule.RemoveAt(blockIndex);
        // Add code for removing on clients later
    }

    /// <summary>
    /// Sends the specified block to all holders of an interest group
    /// </summary>
    /// <param name="block"></param>
    void SendAddBlockData(ScheduleBlock block)
    {
        // Send the schedule block to individual players
        foreach (KeyValuePair<PlayerRef, PlayerManager.PlayerProperties> player in playerManager.playerProperties)
        {
            if (block.assignedPlayers.Contains(player.Key)) // Iterates over every player who is assigned to this block
            {
                RPC_SendScheduleBlock(player.Key, block.periodName, block.room, block.time, block.length, block.assignedPlayers.ToArray(), block.interestGroups.ToArray());
            }
        }
        // Send the schedule block to groups who can see it (interestGroup)
        SendProxyBlock(block);
    }

    void SendRemoveBlockData(ScheduleBlock block)
    {
        foreach (KeyValuePair<PlayerRef, PlayerManager.PlayerProperties> player in playerManager.playerProperties)
        {
            if (block.assignedPlayers.Contains(player.Key)) // Iterates over every player who is assigned to this block
            {
                RPC_RemoveScheduleBlock(player.Key, block.periodName, block.room, block.time, block.length);
            }
        }
        // Send the schedule block to groups who can see it (interestGroup)
        SendProxyRemove(block);
    }

    /// <summary>
    /// Sends schedule block information to the specified player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="periodName"></param>
    /// <param name="room"></param>
    /// <param name="time"></param>
    /// <param name="length"></param>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendScheduleBlock([RpcTarget] PlayerRef player, string periodName, string room, float time, float length, PlayerRef[] assignedPlayers, int[] interest)
    {
        ScheduleBlock newBlock = new ScheduleBlock(periodName, room, length, time, assignedPlayers.ToList(), interest.ToList());
        localSchedule.Add(newBlock);

        OnUpdateSchedule?.Invoke();
    }
    /// <summary>
    /// Removes the specified equivalent schedule block on the player's local schedule
    /// </summary>
    /// <param name="player"></param>
    /// <param name="periodName"></param>
    /// <param name="room"></param>
    /// <param name="time"></param>
    /// <param name="length"></param>

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RemoveScheduleBlock([RpcTarget] PlayerRef player, string periodName, string room, float time, float length)
    {
        ScheduleBlock removedBlock = new ScheduleBlock(periodName, room, length, time);
        ScheduleBlock localRemoved = removedBlock.GetEquivalentBlockInSchedule(localSchedule);
        if (!localRemoved.Equals(ScheduleBlock.None))
        {
            localSchedule.Remove(localRemoved);
        }
    }

    // Sends the proxy block to interest groups
    void SendProxyBlock(ScheduleBlock block)
    {
        if (block.interestGroups == null)
        {
            SendProxyBlockToAll(block);
            return;
        }
        if (block.interestGroups.Count == 0)
        {
            SendProxyBlockToAll(block);
            return;
        }
        // Sends the proxy to all interest groups
        List<PlayerRef> sentPlayers = new List<PlayerRef>();
        foreach (int group in block.interestGroups)
        {
            // Gets all players in the group and iterates
            List<PlayerRef> players = playerManager.GetPlayersInGroup(group);
            foreach (PlayerRef player in players)
            {
                if (sentPlayers.Contains(player)) continue; // To prevent double sending
                sentPlayers.Add(player);
                // Sends to all players in the interest group the individual assigned player's schedule block
                RPC_SendProxyBlock(player, block.periodName, block.room, block.time, block.length, block.assignedPlayers.ToArray(), block.interestGroups.ToArray());
            }
        }
    }

    // For when a block is visible to all players
    void SendProxyBlockToAll(ScheduleBlock block)
    {
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            RPC_SendProxyBlock(player, block.periodName, block.room, block.time, block.length, block.assignedPlayers.ToArray(), block.interestGroups.ToArray());
        }
    }

    /// <summary>
    /// Sends a schedule block to every client relating to a certain player (Info for reading schedules)
    /// </summary>
    /// <param name="player"></param>
    /// <param name="periodName"></param>
    /// <param name="room"></param>
    /// <param name="time"></param>
    /// <param name="length"></param>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendProxyBlock([RpcTarget] PlayerRef player, string periodName, string room, float time, float length, PlayerRef[] assignedPlayers, int[] interestGroups)
    {
        ScheduleBlock sentBlock = new ScheduleBlock(periodName, room, length, time, assignedPlayers.ToList(), interestGroups.ToList());
        foreach (PlayerRef proxy in assignedPlayers) // Updates all proxy schedules on this client
        {
            AddProxyBlock(proxy, sentBlock);
        }
    }

    void SendProxyRemove(ScheduleBlock block)
    {
        if (block.interestGroups == null)
        {
            RemoveProxyBlockFromAll(block);
            return;
        }
        if (block.interestGroups.Count == 0)
        {
            RemoveProxyBlockFromAll(block);
            return;
        }
        // Sends the proxy to all interest groups
        List<PlayerRef> sentPlayers = new List<PlayerRef>();
        foreach (int group in block.interestGroups)
        {
            // Gets all players in the group and iterates
            List<PlayerRef> players = playerManager.GetPlayersInGroup(group);
            foreach (PlayerRef player in players)
            {
                if (sentPlayers.Contains(player)) continue; // To prevent double sending
                sentPlayers.Add(player);
                // Sends to all players in the interest group the individual assigned player's schedule block
                RPC_SendProxyBlock(player, block.periodName, block.room, block.time, block.length, block.assignedPlayers.ToArray(), block.interestGroups.ToArray());
            }
        }
    }

    void RemoveProxyBlockFromAll(ScheduleBlock block)
    {
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            RPC_RemoveProxyBlock(player, block.periodName, block.room, block.time, block.length, block.assignedPlayers.ToArray(), block.interestGroups.ToArray());
        }
    }

    /// <summary>
    /// Removes the proxy block for the proxies from the assigned players. Pass in a subset of assigned players when reassigning a schedule block   
    /// </summary>
    /// <param name="player"></param>
    /// <param name="periodName"></param>
    /// <param name="room"></param>
    /// <param name="time"></param>
    /// <param name="length"></param>
    /// <param name="assignedPlayers"></param>
    /// <param name="interestGroups"></param>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RemoveProxyBlock([RpcTarget] PlayerRef player, string periodName, string room, float time, float length, PlayerRef[] assignedPlayers, int[] interestGroups)
    {
        ScheduleBlock checkedBlock = new ScheduleBlock(periodName, room, length, time, assignedPlayers.ToList(), interestGroups.ToList());
        foreach (PlayerRef proxy in assignedPlayers)
        {
            ScheduleBlock foundBlock = checkedBlock.GetEquivalentBlockInSchedule(playerSchedules[proxy]);
            if (!foundBlock.Equals(ScheduleBlock.None))
            {
                playerSchedules[proxy].Remove(foundBlock);
            }
        }
    }

    public void AddProxyBlock(PlayerRef player, ScheduleBlock proxyBlock)
    {
        proxySchedules[player].Add(proxyBlock);
    }

    // Below is reference code (may be deprecated)
    //[PunRPC]
    public void AddSchedulePlayer(PlayerRef player)
    {
        dplayerSchedules.Add(player, new List<ScheduleBlock>());
    }

    //[PunRPC]
    public void AddScheduleBlock(PlayerRef player, string periodName, string room, float time, float length)
    {
        dplayerSchedules[player].Add(new ScheduleBlock(periodName, room, length, time));

        if (player == Runner.LocalPlayer) dlocalSchedule.Add(new ScheduleBlock(periodName, room, length, time));
        OnUpdateSchedule?.Invoke();
    }

    //[PunRPC]
    public void RemoveScheduleBlock(PlayerRef player, float time)
    {
        int i = 0;
        foreach (ScheduleBlock block in new List<ScheduleBlock>(dplayerSchedules[player])) // Removes from dictionary
        {
            if (block.time == time)
            {
                dplayerSchedules[player].RemoveAt(i);
                break;
            }
            i++;
        }

        // Removes locally
        if (player == Runner.LocalPlayer) dlocalSchedule.RemoveAt(i);
        OnUpdateSchedule?.Invoke();
    }

    //[PunRPC]
    public void ClearSchedule()
    {
        dlocalSchedule.Clear();

        OnUpdateSchedule?.Invoke();
    }

    public bool PeriodOverlaps(float startTime, float endTime, PlayerRef player)
    {
        bool output = false;
        foreach (ScheduleBlock block in dplayerSchedules[player])
        {
            if (block.time > startTime && block.time < endTime)
            {
                output = true; break;
            }
            if (block.time + block.length > startTime && block.time + block.length < endTime)
            {
                output = true; break;
            }
            if (block.time < startTime && block.time + block.length > endTime)
            {
                output = true; break;
            }
        }
        foreach (ScheduleBlock block in dimmutableBlocks)
        {
            if (block.time > startTime && block.time < endTime)
            {
                output = true; break;
            }
            if (block.time + block.length > startTime && block.time + block.length < endTime)
            {
                output = true; break;
            }
            if (block.time < startTime && block.time + block.length > endTime)
            {
                output = true; break;
            }
        }
        return output;
    }

    // Block check code

    void CheckBlockChange()
    {
        if (dorderedBlocks.Count == 0) return;
        while (gm.currentPeriod > dorderedBlocks[0].time + dorderedBlocks[0].length) // Removes anything behind the current time
        {
            dorderedBlocks.RemoveAt(0);
            if (dorderedBlocks.Count == 0)
            {
                dcurrentBlock = ScheduleBlock.None;
                CheckPreviousBlock();
                return;
            }
        }
        if (gm.currentPeriod < dorderedBlocks[0].time) // if the current time is behind the next period
        {
            dcurrentBlock = ScheduleBlock.None;
        }
        else if (!dorderedBlocks[0].Equals(dcurrentBlock))
        {
            dcurrentBlock = new ScheduleBlock(dorderedBlocks[0].periodName.ToString(), dorderedBlocks[0].room.ToString(), dorderedBlocks[0].length, dorderedBlocks[0].time);
        }

        CheckPreviousBlock();
    }

    void CheckPreviousBlock()
    {
        if (!dpreviousBlock.Equals(dcurrentBlock))
        {
            OnBlockChange?.Invoke(dpreviousBlock, dcurrentBlock);
            dpreviousBlock = dcurrentBlock;
        }
    }

    void ResetBlockCheck()
    {
        dcurrentBlock = ScheduleBlock.None;
        dpreviousBlock = ScheduleBlock.None;
    }

    void UpdateOrderedBlocks()
    {
        List<ScheduleBlock> newOrdered = new List<ScheduleBlock>();
        float minRange = gm.currentDay * 24 - 1;
        float maxRange = gm.currentDay * 24 + 23;

        foreach (ScheduleBlock block in dimmutableBlocks)
        {
            newOrdered.Add(new ScheduleBlock(block.periodName.ToString(), block.room.ToString(), block.length, block.time + (gm.currentDay * 24)));
        }

        foreach (ScheduleBlock block in dlocalSchedule)
        {
            if (block.time < minRange || block.time > maxRange) continue;
            newOrdered.Add(new ScheduleBlock(block.periodName.ToString(), block.room.ToString(), block.length, block.time));
        }

        newOrdered = newOrdered.OrderBy(o => o.time).ToList();
        dorderedBlocks = newOrdered;
    }
}
