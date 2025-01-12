using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;

// For client side part of meetings
public class MeetingRoom : MonoBehaviour
{
    public Transform higherSeatHolder;
    public Transform civilianSeatHolder;
    //public PhotonView view;
    GameManager gm;
    PlayerManager pm;
    PlayerMovement playerMovement;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        //pm = FindObjectOfType<PlayerManager>();
        pm.OnInstantiatePlayer += GetReferences;
    }

    void GetReferences(GameObject player)
    {
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    //[PunRPC]
    public void TeleportToSeat(int seat)
    {
        //if ((int)gm.playerPositions[(string)PhotonNetwork.LocalPlayer.CustomProperties["name"]] == 0)
        //{
        //    Transform teleportTransforom = civilianSeatHolder.GetChild(seat);
        //    pm.Teleport(teleportTransforom.position, teleportTransforom.rotation);
        //} 
        //else
        //{
        //    Transform teleportTransforom = higherSeatHolder.GetChild(seat);
        //    pm.Teleport(teleportTransforom.position, teleportTransforom.rotation);
        //}
        //playerMovement.Freeze();
    }
}
