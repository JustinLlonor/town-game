using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;

public class OfflineMode : MonoBehaviour
{
    private void Awake()
    {
        FindFirstObjectByType<PlayerManager>().currentPlayer = transform.parent.gameObject;
        FindFirstObjectByType<FirstPerson>().trackedMV = transform.parent.GetComponent<PlayerMovement>();
        if (FindFirstObjectByType<WaitingRoomManager>() != null) FindFirstObjectByType<WaitingRoomManager>().enabled = false;
        //PhotonNetwork.OfflineMode = true;
    }

    private void Start()
    {
        Debug.Log("invoking");
        FindFirstObjectByType<PlayerManager>().onInstantiatePlayer?.Invoke(transform.parent.gameObject);
    }
}
