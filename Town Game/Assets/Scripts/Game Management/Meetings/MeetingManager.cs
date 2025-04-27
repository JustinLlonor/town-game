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
    public bool meetingStarted = false;
    public MapRoom meetingRoom;

    public override void Spawned()
    {
        Debug.Log("spawned called");
        scheduleManager.OnMasterBlockStart += CheckMeetingStart;
        scheduleManager.OnMasterBlockEnd += CheckMeetingEnd;
        meetingRoom.onPlayerEnter += PlayerEnterMeeting;
        meetingRoom.onPlayerExit += PlayerLeaveMeeting;
    }

    private void CheckMeetingStart(ScheduleBlock block)
    {
        Debug.Log(meetingPeriodName);
        Debug.Log(block.periodName);
        if (block.periodName != meetingPeriodName) return;
        meetingStarted = true;
        // Meeting start code
    }

    private void CheckMeetingEnd(ScheduleBlock block)
    {
        Debug.Log("Master block end called for " + block.periodName);
        if (block.periodName != meetingPeriodName) return;
        meetingStarted = false;
    }

    public void PlayerEnterMeeting(PlayerRef player)
    {
        Debug.Log("this delegate works");
        if (!meetingStarted) return;
        Debug.Log("Player has entered meeting");
    }

    public void PlayerLeaveMeeting(PlayerRef player)
    {
        if (!meetingStarted) return;
        Debug.Log("Player has left meeting");
    }
}
