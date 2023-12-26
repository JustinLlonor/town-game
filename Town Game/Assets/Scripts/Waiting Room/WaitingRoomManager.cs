using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WaitingRoomManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public int playersRequired = 2;
    public float gameTimer;
    public float clientTimer;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameTimer);
        }
        else
        {
            gameTimer = (float)stream.ReceiveNext();
        }
    }
}   
