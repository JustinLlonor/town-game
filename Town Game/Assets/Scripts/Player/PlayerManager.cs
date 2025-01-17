//using Photon.Pun;
//using Photon.Realtime;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour//PunCallbacks
{
    public GameObject currentPlayer;
    public NetworkPrefabRef playerPrefab;
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

    // Delegate for when the player gets spawned
    public InstantiatePlayer OnInstantiatePlayer;
    public PlayerEvent OnTeleportPlayer;
    public delegate void InstantiatePlayer(GameObject player);
    public delegate void PlayerEvent();

    [System.Serializable]
    public class PlayerSettings
    {
        public float speed = 3f;    
        public bool canJump = true;
    }

    private void Start()
    {
        //if (!PhotonNetwork.IsConnected) return;
        //if (PhotonNetwork.CurrentRoom == null) return;
        //GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, spawn.rotation);
        //OnInstantiatePlayer?.Invoke(player);
        //PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        //PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        //playerInventory.camTransform = camTransform;
        //playerInventory.hotbarUI = hotbar;
        //playerInventory.largeUI = largeUI;
        //playerMovement.speed = playerSettings.speed;
        //playerMovement.canJump = playerSettings.canJump;

        //currentPlayer = player;
    }

    public void SetupOnClient(GameObject player)
    {
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

    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        runner.Spawn(playerPrefab, spawn.position, Quaternion.identity, player);
        // Add OnInstantiate later
    }

    //[PunRPC]
    public void Teleport(Vector3 location, Quaternion rotation)
    {
        if (currentPlayer == null) return;

        Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();
        PlayerMovement pm = currentPlayer.GetComponent<PlayerMovement>();
        CameraMovement cm = FindObjectOfType<CameraMovement>();
        currentPlayer.transform.position = location;
        currentPlayer.transform.rotation = rotation;
        pm.cameraPosition.eulerAngles = new Vector3 (0, rotation.eulerAngles.y, 0);
        rb.velocity = Vector3.zero;
        rb.position = location;
        rb.rotation = rotation;
        cm.yRotation = rotation.eulerAngles.y;
        cm.xRotation = rotation.eulerAngles.x;
        OnTeleportPlayer?.Invoke();
    }
}
