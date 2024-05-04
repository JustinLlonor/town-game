using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScheduleManager : MonoBehaviour
{
    public GameManager gm;
    public List<ScheduleBlock> schedule;
    public UpdateSchedule OnUpdateSchedule;

    public delegate void UpdateSchedule();

    [PunRPC]
    public void AddScheduleBlock(string periodName, string room, float time, float length = 1f)
    {
        schedule.Add(new ScheduleBlock(periodName, room, time, length));

        OnUpdateSchedule?.Invoke();
    }

    [PunRPC]
    public void RemoveScheduleBlock(int index)
    {
        schedule.RemoveAt(index);

        OnUpdateSchedule?.Invoke();
    }

    [PunRPC]
    public void ClearSchedule()
    {
        schedule.Clear();

        OnUpdateSchedule?.Invoke();
    }

    bool PeriodOverlaps(float startTime, float endTime)
    {
        bool output = false;
        foreach (ScheduleBlock block in schedule)
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
