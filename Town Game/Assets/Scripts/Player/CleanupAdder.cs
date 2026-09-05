using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanupAdder : MonoBehaviour//PunCallbacks
{
    //Photon.Realtime.Player currentPlayer;
    CleanupMaster cm;

    private void Start()
    {
    //    PhotonView view = gameObject.GetComponent<PhotonView>();
    //    cm = FindObjectOfType<CleanupMaster>();
    //
    //    cm.AddPlayer(view);
    //    currentPlayer = view.Owner;
    }

    private void OnDestroy()
    {
        //if (cm.playerObjects.ContainsKey(currentPlayer)) cm.playerObjects.Remove(currentPlayer);
    }
}
