using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;
using System;

public class ScheduleManager : NetworkBehaviour
{
    public List<ScheduleBlock> masterSchedule = new List<ScheduleBlock>();
    public List<ScheduleBlock> localSchedule;
    public List<ScheduleBlock> dailyBlocks = new List<ScheduleBlock>();
    public Dictionary<PlayerRef, List<ScheduleBlock>> playerSchedules = new Dictionary<PlayerRef, List<ScheduleBlock>>(); // Contains the assigned schedule blocks of every player, to be revealed on other clients
    public Dictionary<PlayerRef, List<ScheduleBlock>> proxySchedules = new Dictionary<PlayerRef, List<ScheduleBlock>>(); // For clients, the schedule blocks that are revealed to them
    [HideInInspector] public List<ScheduleBlock> currentBlocks = new List<ScheduleBlock>();
    [HideInInspector] public List<ScheduleBlock> orderedBlocks = new List<ScheduleBlock>();
    [HideInInspector] public List<ScheduleBlock> currentMasterBlocks = new List<ScheduleBlock>();
    List<ScheduleBlock> orderedMasterBlocks = new List<ScheduleBlock>();

    [HideInInspector] public GameManager gm;

    // Client events
    public ScheduleEvent OnUpdateSchedule;
    public BlockEvent OnBlockStart;
    public BlockEvent OnBlockEnd;
    public PlayerEvent OnProxyScheduleChange;

    // Server events
    public ScheduleEvent OnUpdateMasterSchedule;
    public BlockEvent OnMasterBlockStart;
    public BlockEvent OnMasterBlockEnd;

    public delegate void ScheduleEvent();
    public delegate void BlockEvent(ScheduleBlock block);
    public delegate void PlayerEvent(PlayerRef player);

    PlayerManager playerManager;
    bool init = false;
    
    private void Awake()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
        //view = gameObject.GetComponent<PhotonView>();
        OnUpdateSchedule += UpdateOrderedBlocks; // When schedule updates or when the day changes, ordered blocks is updated
        OnUpdateMasterSchedule += UpdateMasterOrdered;
        gm.OnChangeDay += UpdateOrderedBlocks;
        //view.RPC("AddSchedulePlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
    }

    private void Start()
    {
        //view.RPC("AddScheduleBlock", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, "Patrol", "", 13f, 1f);
    }

    public override void Spawned()
    {
        init = true;
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
    }

    private void Update()
    {
        if (!init) return;
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            AddBlock("Blackmarket", "Random Alleyway", 7f, 1f, Color.red, new List<PlayerRef>() { Runner.LocalPlayer });
            AddBlock("Patrol Town", "", 7f, 3f, Color.green, new List<PlayerRef>() { Runner.LocalPlayer });
            AddBlock("Election", "", 12f, 2f, Color.cyan, new List<PlayerRef>() { Runner.LocalPlayer });
            AddBlock("Judgement", "", 12f, 2f, Color.yellow, new List<PlayerRef>() { Runner.LocalPlayer });
        }
        CheckBlockChanges();
        CheckMasterBlockChanges();
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
    public ScheduleBlock AddBlock(string periodName, string room, float time, float length, Color color, List<PlayerRef> assignedPlayers, List<int> interestGroups = null)
    {
        if (interestGroups == null) interestGroups = new List<int>();
        ScheduleBlock newBlock = new ScheduleBlock(periodName, room, length, time, color, assignedPlayers, interestGroups);
        masterSchedule.Add(newBlock);
        OnUpdateMasterSchedule?.Invoke();
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
        OnUpdateMasterSchedule?.Invoke();
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
                int colorInt = ColorToInt(block.color);
                RPC_SendScheduleBlock(player.Key, block.periodName, block.room, block.time, block.length, colorInt, block.assignedPlayers.ToArray(), block.interestGroups.ToArray());
            }
        }
        // Send the schedule block to groups who can see it (interestGroup)
        SendToInterests(block);
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
        RemoveFromInterests(block);
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
    public void RPC_SendScheduleBlock([RpcTarget] PlayerRef player, string periodName, string room, float time, float length, int color, PlayerRef[] assignedPlayers, int[] interest)
    {
        // Code for converting the integer to a hex color
        Color blockColor = IntToColor(color);

        ScheduleBlock newBlock = new ScheduleBlock(periodName, room, length, time, blockColor, assignedPlayers.ToList(), interest.ToList());
        localSchedule.Add(newBlock);
        proxySchedules[Runner.LocalPlayer] = localSchedule;
        OnProxyScheduleChange?.Invoke(Runner.LocalPlayer);
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
        ScheduleBlock removedBlock = new ScheduleBlock(periodName, room, length, time, Color.white);
        ScheduleBlock localRemoved = removedBlock.GetEquivalentBlockInSchedule(localSchedule);
        if (!localRemoved.Equals(ScheduleBlock.None))
        {
            localSchedule.Remove(localRemoved);
            proxySchedules[Runner.LocalPlayer] = localSchedule;
            OnProxyScheduleChange?.Invoke(Runner.LocalPlayer);
            OnUpdateSchedule?.Invoke();
        }
    }

    // Sends the proxy block to interest groups
    void SendToInterests(ScheduleBlock block)
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
                RPC_SendProxyBlock(player, block.periodName, block.room, block.time, block.length, ColorToInt(block.color), block.assignedPlayers.ToArray(), block.interestGroups.ToArray());
            }
        }
    }

    // For when a block is visible to all players
    void SendProxyBlockToAll(ScheduleBlock block)
    {
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            RPC_SendProxyBlock(player, block.periodName, block.room, block.time, block.length, ColorToInt(block.color), block.assignedPlayers.ToArray(), block.interestGroups.ToArray());
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
    public void RPC_SendProxyBlock([RpcTarget] PlayerRef player, string periodName, string room, float time, float length, int color, PlayerRef[] assignedPlayers, int[] interestGroups)
    {
        Color blockColor = IntToColor(color);
        ScheduleBlock sentBlock = new ScheduleBlock(periodName, room, length, time, blockColor, assignedPlayers.ToList(), interestGroups.ToList());
        foreach (PlayerRef proxy in assignedPlayers) // Updates all proxy schedules on this client
        {
            if (proxy == Runner.LocalPlayer) continue;
            proxySchedules[proxy].Add(sentBlock);
            OnProxyScheduleChange?.Invoke(proxy);
        }
    }
    
    void RemoveFromInterests(ScheduleBlock block)
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
                RPC_SendProxyBlock(player, block.periodName, block.room, block.time, block.length, ColorToInt(block.color), block.assignedPlayers.ToArray(), block.interestGroups.ToArray());
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
        ScheduleBlock checkedBlock = new ScheduleBlock(periodName, room, length, time);
        foreach (PlayerRef proxy in assignedPlayers)
        {
            if (proxy == Runner.LocalPlayer) continue;
            ScheduleBlock foundBlock = checkedBlock.GetEquivalentBlockInSchedule(playerSchedules[proxy]);
            if (!foundBlock.Equals(ScheduleBlock.None))
            {
                playerSchedules[proxy].Remove(foundBlock);
                OnProxyScheduleChange?.Invoke(proxy);
            }
        }
    }

    void CheckBlockChanges()
    {
        while (orderedBlocks.Count > 0 && gm.currentPeriod > orderedBlocks[0].time + orderedBlocks[0].length) // Removes anything behind the current time
        {
            orderedBlocks.RemoveAt(0);
        }

        // Add all periods to newBlocks
        List<ScheduleBlock> newBlocks = new List<ScheduleBlock>();
        for (int i = 0; i < orderedBlocks.Count; i++)
        {
            ScheduleBlock cBlock = orderedBlocks[i];
            if (gm.currentPeriod >= cBlock.time && gm.currentPeriod <= (cBlock.length + cBlock.time)) // If the current period is within
            {
                newBlocks.Add(cBlock);
            }
        }

        List<ScheduleBlock> addedBlocks = new List<ScheduleBlock>();
        // Find added blocks
        foreach (ScheduleBlock block in newBlocks)
        {
            if (!currentBlocks.Contains(block))
            {
                addedBlocks.Add(block);
            }
        }

        List<ScheduleBlock> removedBlocks = new List<ScheduleBlock>();
        // Find removed blocks
        foreach (ScheduleBlock block in currentBlocks)
        {
            if (!newBlocks.Contains(block))
            {
                removedBlocks.Add(block);
            }
        }

        // If diffBlock is not empty, invoke change
        CheckDelegate(addedBlocks, OnBlockStart);
        CheckDelegate(removedBlocks, OnBlockEnd);

        currentBlocks = newBlocks; // Current block list becomes new block list
    }

    // Invokes the specified event for every block
    void CheckDelegate(List<ScheduleBlock> blocks, BlockEvent bl)
    {
        if (blocks.Count == 0) return;
        foreach (ScheduleBlock block in blocks)
        {
            bl?.Invoke(block);
        }
    }

    void UpdateOrderedBlocks()
    {
        // Client
        List<ScheduleBlock> newOrdered = new List<ScheduleBlock>();
        float minRange = gm.currentDay * 24 - 1;
        float maxRange = gm.currentDay * 24 + 23;

        foreach (ScheduleBlock block in localSchedule)
        {
            if (block.time < minRange || block.time > maxRange) continue;
            newOrdered.Add(new ScheduleBlock(block.periodName, block.room, block.length, block.time, block.color));
        }

        foreach (ScheduleBlock block in dailyBlocks)
        {
            newOrdered.Add(new ScheduleBlock(block.periodName, block.room, block.length, gm.currentDay * 24 + block.time, block.color));
        }

        newOrdered = newOrdered.OrderBy(o => o.time).ToList();
        orderedBlocks = newOrdered;
    }

    void UpdateMasterOrdered()
    {
        orderedMasterBlocks = masterSchedule.OrderBy(o => o.time).ToList();
    }

    void CheckMasterBlockChanges()
    {
        if (!Runner.IsServer) return;
        while (orderedMasterBlocks.Count > 0 && gm.currentPeriod > orderedMasterBlocks[0].time + orderedMasterBlocks[0].length) // Removes anything behind the current time
        {
            orderedMasterBlocks.RemoveAt(0);
        }

        // Add all periods to newBlocks
        List<ScheduleBlock> newBlocks = new List<ScheduleBlock>();
        for (int i = 0; i < orderedMasterBlocks.Count; i++)
        {
            ScheduleBlock cBlock = orderedMasterBlocks[i];
            if (gm.currentPeriod >= cBlock.time && gm.currentPeriod <= (cBlock.length + cBlock.time)) // If the current period is within
            {
                newBlocks.Add(cBlock);
            }
        }

        List<ScheduleBlock> addedBlocks = new List<ScheduleBlock>();
        // Find added blocks
        foreach (ScheduleBlock block in newBlocks)
        {
            if (!currentMasterBlocks.Contains(block))
            {
                addedBlocks.Add(block);
            }
        }

        List<ScheduleBlock> removedBlocks = new List<ScheduleBlock>();
        // Find removed blocks
        foreach (ScheduleBlock block in currentMasterBlocks)
        {
            if (!newBlocks.Contains(block))
            {
                removedBlocks.Add(block);
            }
        }

        // If diffBlock is not empty, invoke change
        CheckDelegate(addedBlocks, OnMasterBlockStart);
        CheckDelegate(removedBlocks, OnMasterBlockEnd);

        currentMasterBlocks = newBlocks; // Current block list becomes new block list
    }

    Color IntToColor(int colorInt)
    {
        string htmlValue = colorInt.ToString("X");
        while (htmlValue.Length < 6) htmlValue = "0" + htmlValue; // Adds leading 0s
        Color blockColor;
        ColorUtility.TryParseHtmlString("#" + htmlValue, out blockColor);
        return blockColor;
    }

    int ColorToInt(Color color)
    {
        string hexString = ColorUtility.ToHtmlStringRGB(color);
        int colorInt = int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        return colorInt;
    }
}
