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
    public CleanupMaster cm;
    public FirstPerson fps;

    public InstantiatePlayer OnInstantiatePlayer;
    public delegate void InstantiatePlayer(GameObject player);

    [System.Serializable]
    public class PlayerSettings
    {
        public float speed = 3f;    
        public bool canJump = true;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected) return;
        if (PhotonNetwork.CurrentRoom == null) return;
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, spawn.rotation);
        OnInstantiatePlayer?.Invoke(player);
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        playerInventory.camTransform = camTransform;
        playerInventory.hotbarUI = hotbar;
        playerInventory.largeUI = largeUI;
        playerMovement.speed = playerSettings.speed;
        playerMovement.canJump = playerSettings.canJump;

        currentPlayer = player;
    }
}
