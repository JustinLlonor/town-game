using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class MeetingManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool meetingQueued = false;
    public string mealtimeRoom = "Cafeteria";
    ScheduleManager sm;
    AnnouncementManager am;
    GameManager gm;
    PhotonView view;
    MeetingRoom meetingRoom;

    public MeetingEvent OnMeetingStart;
    public MeetingEvent OnMeetingEnd;
    public delegate void MeetingEvent();

    void Awake()
    {
        sm = FindObjectOfType<ScheduleManager>();
        am = FindObjectOfType<AnnouncementManager>();
        gm = FindObjectOfType<GameManager>();
        meetingRoom = FindObjectOfType<MeetingRoom>();
        view = gameObject.GetComponent<PhotonView>();
        sm.OnBlockChange += CheckQueue;
        OnMeetingStart += gm.StopTime;
        OnMeetingEnd += gm.ResumeTime;
    }

    // Starts the queue
    [PunRPC]
    public void QueueMeeting(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (meetingQueued) return;
        meetingQueued = true;
        string senderName = (string)info.Sender.CustomProperties["name"];
        am.Announce($"{senderName} has queued a meeting.");
    }

    // Check if mealtime has ended, then start meeting
    void CheckQueue(ScheduleBlock from, ScheduleBlock to)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!meetingQueued) return;
        // admire the spaghetti in all its glory, and also fuck you because i ain't making this look better
        if (from != null)
        {
            if (from.room != mealtimeRoom)
            {
                return;
            } else
            {
                if (from.periodName != "Brunch" && from.periodName != "Dinner") return; //change this if the period changes
            }
        } else
        {
            return;
        }
        meetingQueued = false;
        view.RPC("StartMeeting", RpcTarget.All);
    }

    void SetupSeats()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        List<int> takenCivilianSeats = new List<int>(new int[meetingRoom.civilianSeatHolder.childCount]);
        for (int i = 0; i < takenCivilianSeats.Count; i++) takenCivilianSeats[i] = i;
        foreach (Player player in gm.alivePlayers)
        {
            // if not civilian
            if ((int)gm.playerPositions[(string)player.CustomProperties["name"]] > 0) continue;
            int newSeat = Random.Range(0, takenCivilianSeats.Count);
            Debug.LogWarning(takenCivilianSeats.Count);
            meetingRoom.view.RPC("TeleportToSeat", player, takenCivilianSeats[newSeat]);
            takenCivilianSeats.RemoveAt(newSeat);
        }
        List<int> takenHigherSeats = new List<int>(new int[meetingRoom.higherSeatHolder.childCount]);
        for (int i = 0; i < takenHigherSeats.Count; i++) takenHigherSeats[i] = i;
        foreach (Player player in gm.alivePlayers)
        {
            // if civilian
            if ((int)gm.playerPositions[(string)player.CustomProperties["name"]] == 0) continue;
            int newSeat = Random.Range(0, takenHigherSeats.Count);
            meetingRoom.view.RPC("TeleportToSeat", player, takenHigherSeats[newSeat]);
            takenHigherSeats.RemoveAt(newSeat);
        }
    }

    // what do you think it does smartass
    [PunRPC]
    public void StartMeeting()
    {
        SetupSeats();
        OnMeetingStart?.Invoke();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(meetingQueued);
        } 
        else
        {
            meetingQueued = (bool)stream.ReceiveNext();
        }
    }
}
