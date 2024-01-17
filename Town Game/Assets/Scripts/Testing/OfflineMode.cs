using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OfflineMode : MonoBehaviour
{
    private void Awake()
    {
        FindObjectOfType<PlayerManager>().currentPlayer = transform.parent.gameObject;
        PhotonNetwork.OfflineMode = true;
    }
}
