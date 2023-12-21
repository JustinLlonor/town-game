using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

    private void Start()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Instantiating");
            PhotonNetwork.InstantiateRoomObject("Player", Vector3.zero, Quaternion.identity);
        }
    }
}
