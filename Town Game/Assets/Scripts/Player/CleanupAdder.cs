using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CleanupAdder : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonView view = gameObject.GetComponent<PhotonView>();

        FindObjectOfType<CleanupMaster>().AddPlayer(view);
    }
}
