[System.Serializable]
public class ScheduleBlock
{
    // The name of the activity
    public string periodName;
    // The place of the activity, is empty if the activity is open-ended
    public string room;
    // Length of the block in periods
    public float length;
    // Start time of the block
    public float time;

    public ScheduleBlock(string periodName, string room, float length, float time)
    {
        this.periodName = periodName;
        this.room = room;
        this.length = length;
        this.time = time;
    }

    public override bool Equals(object obj)
    {
        ScheduleBlock block = obj as ScheduleBlock;

        if (block == null) return false;

        if (periodName != block.periodName) return false;
        if (room != block.room) return false;
        if (length != block.length) return false;
        if (time != block.time) return false;

        return true;
    }
}
