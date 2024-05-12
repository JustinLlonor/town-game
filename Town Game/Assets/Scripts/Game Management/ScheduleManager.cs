using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ScheduleManager : MonoBehaviour
{
    public GameManager gm;
    // Dictionary for the schedules of each player
    public Dictionary<Photon.Realtime.Player, List<ScheduleBlock>> playerSchedules = new Dictionary<Photon.Realtime.Player, List<ScheduleBlock>>();
    // this client's schedule
    public List<ScheduleBlock> schedule;
    public List<ScheduleBlock> immutableBlocks = new List<ScheduleBlock>();
    public UpdateSchedule OnUpdateSchedule;
    PhotonView view;

    public delegate void UpdateSchedule();

    private void Awake()
    {
        view = gameObject.GetComponent<PhotonView>();
        view.RPC("AddSchedulePlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
    }

    private void Start()
    {
        view.RPC("AddScheduleBlock", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.CustomProperties["name"], "booty cheeks", 13f, 1f);
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
}
