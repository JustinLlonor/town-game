using Fusion;
using Fusion.Sockets;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [Networked, Capacity(20)] public NetworkDictionary<PlayerRef, NetworkObject> playerObjects => default;
    
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

    private NetworkRunner networkRunner;

    [System.Serializable]
    public class PlayerSettings
    {
        public float speed = 3f;    
        public bool canJump = true;
    }

    void Awake()
    {
        
    }

    private void Start()
    {
        networkRunner = FindObjectOfType<NetworkRunner>();
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

    private void Update()
    {
        if (!networkRunner.IsServer)
        {
            if (Physics.simulationMode == SimulationMode.Script)
            {
                //Physics.simulationMode = SimulationMode.FixedUpdate;
            }
        }
    }

    public void SetupMovementSettings(GameObject player)
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.speed = playerSettings.speed;
        playerMovement.canJump = playerSettings.canJump;
    }

    public void SetupOnClient(GameObject player)
    {
        OnInstantiatePlayer?.Invoke(player);
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        playerInventory.camTransform = camTransform;
        playerInventory.hotbarUI = hotbar;
        playerInventory.largeUI = largeUI;
        playerInventory.Setup();

        currentPlayer = player;
    }

    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        NetworkObject playerObject = runner.Spawn(playerPrefab, spawn.position, Quaternion.identity, player);
        playerObjects.Add(player, playerObject);
        // Add OnInstantiate later
    }

    public void RemovePlayer(PlayerRef player)
    {
        Runner.Despawn(playerObjects[player]);
        playerObjects.Remove(player);
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
