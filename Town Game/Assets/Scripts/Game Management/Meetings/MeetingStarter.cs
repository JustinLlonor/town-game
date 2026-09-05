using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;

public class MeetingStarter : MonoBehaviour
{
    public MeetingManager mm;

    private void Awake()
    {
        mm = FindFirstObjectByType<MeetingManager>();
    }

    public void InitiateMeeting()
    {
        //if (mm.meetingQueued) return;
        //mm.GetComponent<PhotonView>().RPC("QueueMeeting", RpcTarget.MasterClient);
    }

    public void InitiateTestMeeting()
    {
        //if (mm.meetingQueued) return;
        //mm.QueueMeeting();
    }
}
