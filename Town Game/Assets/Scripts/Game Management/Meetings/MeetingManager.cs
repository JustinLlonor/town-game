using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;

public class MeetingManager : NetworkBehaviour
{
    public string meetingPeriodName = "Meeting";
    public ScheduleManager scheduleManager;
    public VotingManager votingManager;

    public override void Spawned()
    {
        scheduleManager.OnMasterBlockStart += CheckMeetingStart;
        scheduleManager.OnMasterBlockEnd += CheckMeetingEnd;
    }

    private void CheckMeetingStart(ScheduleBlock block)
    {
        if (block.periodName != meetingPeriodName) return;
        // Meeting start code
    }

    private void CheckMeetingEnd(ScheduleBlock block)
    {
        if (block.periodName != meetingPeriodName) return;
    }
}
