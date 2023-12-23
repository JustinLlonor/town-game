using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawn;
    public PlayerSettings playerSettings;

    [System.Serializable]
    public class PlayerSettings
    {
        public float speed = 5f;
        public bool canJump = true;
    }

    private void Start()
    {
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, spawn.rotation);
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.speed = playerSettings.speed;
        playerMovement.canJump = playerSettings.canJump;
    }
}
