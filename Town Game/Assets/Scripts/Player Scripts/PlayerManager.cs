using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawn;
    public PlayerSettings playerSettings;
    public Transform camTransform;

    [System.Serializable]
    public class PlayerSettings
    {
        public float speed = 3f;
        public float airspeed = 2.5f;
        public bool canJump = true;
    }

    private void Awake()
    {
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, spawn.rotation);
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        player.GetComponent<PlayerInventory>().camTransform = camTransform;
        playerMovement.speed = playerSettings.speed;
        playerMovement.airSpeed = playerSettings.airspeed;
        playerMovement.canJump = playerSettings.canJump;
    }
}
