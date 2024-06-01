using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// For client side part of meetings
public class MeetingRoom : MonoBehaviour
{
    public Transform higherSeatHolder;
    public Transform civilianSeatHolder;
    public PhotonView view;
    GameManager gm;
    PlayerManager pm;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
    }

    [PunRPC]
    public void TeleportToSeat(int seat)
    {
        if ((int)gm.playerPositions[(string)PhotonNetwork.LocalPlayer.CustomProperties["name"]] == 0)
        {
            Transform teleportTransforom = civilianSeatHolder.GetChild(seat);
            pm.Teleport(teleportTransforom.position, teleportTransforom.rotation);
        } 
        else
        {
            Transform teleportTransforom = higherSeatHolder.GetChild(seat);
            pm.Teleport(teleportTransforom.position, teleportTransforom.rotation);
        }
    }
}
