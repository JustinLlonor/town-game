using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public GameObject currentPlayer;
    public GameObject playerPrefab;
    public Transform spawn;

    public PlayerSettings playerSettings;
    [Header("Assignables")]
    public Transform camTransform;
    public CameraBobbing camBobbing;
    public CameraShake camShake;
    public RectTransform hotbar;
    public GameObject largeUI;

    [System.Serializable]
    public class PlayerSettings
    {
        public float speed = 3f;    
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
        playerInventory.largeUI = largeUI;
        playerMovement.speed = playerSettings.speed;
        playerMovement.canJump = playerSettings.canJump;

        currentPlayer = player;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left");
    }
}
