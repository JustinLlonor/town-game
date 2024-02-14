using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WaitingRoomManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // 0 = not ready 1 = ready 2 = starting
    public int roomPhase = 0;
    public int playersRequired = 2;
    // Objects that correspond to a gamephase
    public GameObject[] phaseUI;
    public GameObject hostUI;
    GameTimer gt;
    int previousRoomPhase = 0;

    public void LaunchGame()
    {

    }

    void Awake()
    {
        gt = gameObject.GetComponent<GameTimer>();
    }

    void Update()
    {
        PhaseUI();
        if (!PhotonNetwork.IsMasterClient) return;
        PhaseLogic();
    }

    void PhaseLogic()
    {
        if (roomPhase == 0)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= playersRequired)
            {
                roomPhase = 1;
            }
            if (PhotonNetwork.IsMasterClient) hostUI.SetActive(false);
        }
        if (roomPhase == 1)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount < playersRequired)
            {
                roomPhase = 0;
                return;
            }
            if (PhotonNetwork.IsMasterClient)
            {
                hostUI.SetActive(true);
                if (Input.GetKeyDown(KeyCode.T))
                {
                    roomPhase = 2;
                    StartTimer();
                    return;
                }
            }
        }
        if (roomPhase == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                hostUI.SetActive(false);
                if (Input.GetKeyDown(KeyCode.T))
                {
                    roomPhase = 1;
                    gt.StopTimer();
                }
            }
        }
    }

    void StartTimer()
    {
        Debug.Log("meow");
        gt.StartTimer(5f);
    }

    void PhaseUI()
    {
        if (previousRoomPhase == roomPhase) return;
        previousRoomPhase = roomPhase;

        foreach (GameObject ui in phaseUI)
        {
            ui.SetActive(false);
        }
        phaseUI[roomPhase].SetActive(true);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if (stream.IsWriting)
        {
            stream.SendNext(roomPhase);
        } else
        {
            roomPhase = (int)stream.ReceiveNext();
        }
    }
}   
