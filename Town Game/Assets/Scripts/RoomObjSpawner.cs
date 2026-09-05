using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;

public class RoomObjSpawner : MonoBehaviour//PunCallbacks
{
    public GameObject[] spawnedPrefabs;

    private void Start()
    {
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    foreach (GameObject obj in spawnedPrefabs)
        //    {
        //        PhotonNetwork.InstantiateRoomObject(obj.name, Vector3.zero, Quaternion.identity);
        //    }
        //}
    }
}
