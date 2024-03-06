using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

public class CleanupMaster : MonoBehaviourPunCallbacks
{
    public Dictionary<Player, PhotonView> playerObjects = new Dictionary<Player, PhotonView> { };

    public void AddPlayer(PhotonView view)
    {
        playerObjects.Add(view.Owner, view);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerObjects.ContainsKey(otherPlayer))
        {
            if (playerObjects[otherPlayer].IsMine)
            {
                PhotonNetwork.Destroy(playerObjects[otherPlayer]);
            }
            playerObjects.Remove(otherPlayer);
        }
    }
}
