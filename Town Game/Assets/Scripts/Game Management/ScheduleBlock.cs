using Fusion;
using System.Collections.Generic;

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

    public ScheduleBlock(string periodName, string room, float length, float time, List<PlayerRef> assignedPlayers = null, List<int> interestGroups = null)
    {
        this.periodName = periodName;
        this.room = room;
        this.length = length;
        this.time = time;
        this.assignedPlayers = assignedPlayers;
        this.interestGroups = interestGroups;
    }

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

    public static ScheduleBlock None
    {
        get
        {
            ScheduleBlock result = new ScheduleBlock(null, null, -1f, -1f, null);
            return result;
        }
    }
}
