using Fusion;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ScheduleBlock
{
    // The name of the activity
    public string periodName;
    // The place of the activity, is empty if the activity is open-ended
    public string room;
    // Length of the block in periods
    public float length;
    // Start time of the block
    public float time;
    // Color of the block
    public Color color;
    /// <summary>
    /// The players this schedule block is assigned to. This schedule block will appear under their name
    /// </summary>
    public List<PlayerRef> assignedPlayers;
    /// <summary>
    /// The groups this schedule block is visible to. If empty, this schedule block is visble to everyone
    /// </summary>
    public List<int> interestGroups;
    // If the schedule is only visible to the player (program later when needed)
    // public bool isPrivate

    public ScheduleBlock(string periodName, string room, float length, float time, Color color = new Color(), List<PlayerRef> assignedPlayers = null, List<int> interestGroups = null)
    {
        this.periodName = periodName;
        this.room = room;
        this.length = length;
        this.time = time;
        this.color = color;
        this.assignedPlayers = assignedPlayers;
        this.interestGroups = interestGroups;
    }

    /// <summary>
    /// Checks if a schedule block is equal to another schedule block. Only checks the period name, room, length, and time, but not the assignedPlayers and interest groups.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if (!(obj is ScheduleBlock)) return false;
        ScheduleBlock block = (ScheduleBlock)obj;

        if (periodName != block.periodName) return false;
        if (room != block.room) return false;
        if (length != block.length) return false;
        if (time != block.time) return false;

        return true;
    }

    /// <summary>
    /// Checks if this schedule block is a job block or not (contains only job groups)
    /// </summary>
    /// <returns></returns>
    public bool isJobBlock()
    {
        if (interestGroups.Count == 0) return false;
        foreach (int group in interestGroups)
        {
            if (group < 0) return false;
        }
        return true;
    }

    /// <summary>
    /// Gets the equivalent block in the specified schedule
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public ScheduleBlock GetEquivalentBlockInSchedule(List<ScheduleBlock> schedule)
    {
        foreach (ScheduleBlock scheduleBlock in schedule)
        {
            if (this.Equals(scheduleBlock))
            {
                return scheduleBlock;
            }
        }
        return ScheduleBlock.None;
    }

    public bool IsContainedWithinSchedule(List<ScheduleBlock> schedule)
    {
        foreach (ScheduleBlock scheduleBlock in schedule)
        {
            if (this.Equals(scheduleBlock))
            {
                return true;
            }
        }
        return false;
    }

    public static ScheduleBlock None
    {
        get
        {
            ScheduleBlock result = new ScheduleBlock(null, null, -1f, -1f);
            return result;
        }
    }
}
