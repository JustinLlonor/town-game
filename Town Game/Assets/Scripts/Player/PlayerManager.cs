using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform spawn;

    public PlayerSettings playerSettings;
    [Header("Assignables")]
    public Transform camTransform;
    public CameraBobbing camBobbing;
    public CameraShake camShake;
    public RectTransform hotbar;

    [System.Serializable]
    public class PlayerSettings
    {
        public float speed = 3f;
        public float airspeed = 2.5f;
        public bool canJump = true;
    }

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected) return;
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, spawn.rotation);
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        playerInventory.camTransform = camTransform;
        playerInventory.hotbarUI = hotbar;
        playerMovement.speed = playerSettings.speed;
        playerMovement.airSpeed = playerSettings.airspeed;
        playerMovement.canJump = playerSettings.canJump;
    }
}
