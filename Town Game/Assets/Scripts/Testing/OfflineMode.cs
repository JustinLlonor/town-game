using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OfflineMode : MonoBehaviour
{
    private void Awake()
    {
        FindObjectOfType<PlayerManager>().currentPlayer = transform.parent.gameObject;
        FindObjectOfType<FirstPerson>().trackedMV = transform.parent.GetComponent<PlayerMovement>();
        if (FindObjectOfType<WaitingRoomManager>() != null) FindObjectOfType<WaitingRoomManager>().enabled = false;
        PhotonNetwork.OfflineMode = true;
    }
}
