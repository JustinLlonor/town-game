using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class ScheduleManager : MonoBehaviour
{
    public GameManager gm;
    // Dictionary for the schedules of each player
    public Dictionary<Photon.Realtime.Player, List<ScheduleBlock>> playerSchedules = new Dictionary<Photon.Realtime.Player, List<ScheduleBlock>>();
    // this client's schedule
    public List<ScheduleBlock> schedule;
    public List<ScheduleBlock> immutableBlocks = new List<ScheduleBlock>();
    public List<ScheduleBlock> orderedBlocks = new List<ScheduleBlock>();
    PhotonView view;
    public ScheduleBlock currentBlock = null;
    ScheduleBlock previousBlock = null;

    public UpdateSchedule OnUpdateSchedule;
    public BlockChange OnBlockChange;
    public delegate void UpdateSchedule();
    /// <summary>
    /// Called when a schedule block ends and another begins
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public delegate void BlockChange(ScheduleBlock from, ScheduleBlock to);

    private void Awake()
    {
        view = gameObject.GetComponent<PhotonView>();
        OnUpdateSchedule += UpdateOrderedBlocks;
        gm.OnChangeDay += ResetBlockCheck;
        gm.OnChangeDay += UpdateOrderedBlocks;
        view.RPC("AddSchedulePlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
    }

    private void Start()
    {
        //view.RPC("AddScheduleBlock", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, "Patrol", "", 13f, 1f);
    }

    private void Update()
    {
        CheckBlockChange();
    }

    [PunRPC]
    public void AddSchedulePlayer(Photon.Realtime.Player player)
    {
        playerSchedules.Add(player, new List<ScheduleBlock>());
    }

    [PunRPC]
    public void AddScheduleBlock(Photon.Realtime.Player player, string periodName, string room, float time, float length)
    {
        if (PeriodOverlaps(time, time + length, player))
        {
            Debug.LogError("Period overlaps, schedule block was not added");
            return;
        }
        playerSchedules[player].Add(new ScheduleBlock(periodName, room, length, time));

        if (player == PhotonNetwork.LocalPlayer) schedule.Add(new ScheduleBlock(periodName, room, length, time));
        OnUpdateSchedule?.Invoke();
    }

    [PunRPC]
    public void RemoveScheduleBlock(Photon.Realtime.Player player, float time)
    {
        int i = 0;
        foreach (ScheduleBlock block in new List<ScheduleBlock>(playerSchedules[player])) // Removes from dictionary
        {
            if (block.time == time)
            {
                playerSchedules[player].RemoveAt(i);
                break;
            }
            i++;
        }

        // Removes locally
        if (player == PhotonNetwork.LocalPlayer) schedule.RemoveAt(i);
        OnUpdateSchedule?.Invoke();
    }

    [PunRPC]
    public void ClearSchedule()
    {
        schedule.Clear();

        OnUpdateSchedule?.Invoke();
    }

    public bool PeriodOverlaps(float startTime, float endTime, Photon.Realtime.Player player)
    {
        bool output = false;
        foreach (ScheduleBlock block in playerSchedules[player])
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
        foreach (ScheduleBlock block in immutableBlocks)
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
        if (orderedBlocks.Count == 0) return;
        while (gm.currentPeriod > orderedBlocks[0].time + orderedBlocks[0].length) // Removes anything behind the current time
        {
            orderedBlocks.RemoveAt(0);
            if (orderedBlocks.Count == 0)
            {
                currentBlock = null;
                CheckPreviousBlock();
                return;
            }
        }
        if (gm.currentPeriod < orderedBlocks[0].time) // if the current time is behind the next period
        {
            currentBlock = null;
        }
        else if (!orderedBlocks[0].Equals(currentBlock))
        {
            currentBlock = new ScheduleBlock(orderedBlocks[0].periodName, orderedBlocks[0].room, orderedBlocks[0].length, orderedBlocks[0].time);
        }

        CheckPreviousBlock();
    }

    void CheckPreviousBlock()
    {
        if (previousBlock != currentBlock)
        {
            OnBlockChange?.Invoke(previousBlock, currentBlock);
            previousBlock = currentBlock;
        }
    }

    void ResetBlockCheck()
    {
        currentBlock = null;
        previousBlock = null;
    }

    void UpdateOrderedBlocks()
    {
        List<ScheduleBlock> newOrdered = new List<ScheduleBlock>();
        float minRange = gm.currentDay * 24 - 1;
        float maxRange = gm.currentDay * 24 + 23;

        foreach (ScheduleBlock block in immutableBlocks)
        {
            newOrdered.Add(new ScheduleBlock(block.periodName, block.room, block.length, block.time + (gm.currentDay * 24)));
        }

        foreach (ScheduleBlock block in schedule)
        {
            if (block.time < minRange || block.time > maxRange) continue;
            newOrdered.Add(new ScheduleBlock(block.periodName, block.room, block.length, block.time));
        }

        newOrdered = newOrdered.OrderBy(o => o.time).ToList();
        orderedBlocks = newOrdered;
    }
}
