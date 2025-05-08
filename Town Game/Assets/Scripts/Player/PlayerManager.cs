using Fusion;
using Fusion.Sockets;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public bool spawnPlayersOnJoin = true;
    [Networked, Capacity(20)] public NetworkDictionary<PlayerRef, NetworkId> playerObjects => default;
    public Dictionary<PlayerRef, Observable> playerObservables = new Dictionary<PlayerRef, Observable>();
    public Dictionary<PlayerRef, PlayerProperties> playerProperties = new Dictionary<PlayerRef, PlayerProperties>();
    public PlayerProperties currentPlayerProperties = new PlayerProperties("", false, 0, 0, 0);
    private Dictionary<PlayerRef, TeleportInfo> teleportQueue = new Dictionary<PlayerRef, TeleportInfo>();
    
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

    public bool removePlayers = false;

    [System.Serializable]
    public class PlayerSettings
    {
        public float speed = 3f;    
        public bool canJump = true;
    }

    /// <summary>
    /// Properties of players that are not constantly streamed or networked
    /// </summary>
    public class PlayerProperties
    {
        public string nickname;
        public bool isCultist;
        public int room;
        public int currency;
        public List<int> groups; // -1 = Cultist, -2 = Innocent, 0 and above are job indices
        public int energy;
        public int branch; // Index of the branch
        public List<Vector2Int> jobs = new List<Vector2Int>(); // List of job refs

        public PlayerProperties(string nickname, bool isCultist, int room, int currency, int branch)
        {
            this.nickname = nickname;
            this.isCultist = isCultist;
            this.room = room;
            this.currency = currency;
            this.branch = branch;
        }

        public void SetIsCultist(bool newIsCultist)
        {
            isCultist = newIsCultist;
        }

        public void SetBranch(int branch)
        {
            this.branch = branch;
        }

        public void SetRoom(int newRoom)
        {
            this.room = newRoom;
        }

        public void SetCurrency(int newCurrency)
        {
            currency = newCurrency;
        }

        public void SetEnergy(int newEnergy)
        {
            energy = newEnergy;
        }

        public void AddJob(Vector2Int jobRef)
        {
            if (jobs.Contains(jobRef)) return;
            jobs.Add(jobRef);
        }

        public void RemoveJob(Vector2Int jobRef)
        {
            if (!jobs.Contains(jobRef)) return;
            jobs.Remove(jobRef);
        }

        /// <summary>
        /// Checks if this player is part of a list of groups
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public bool IsPartOfGroups(List<int> groups)
        {
            if (groups == null) return true;
            foreach (int group in groups)
            {
                if (this.groups.Contains(group)) return true; // If the groups in this instance contain a group in the gorups instance, return true
            }
            return false;
        }

        public bool IsPartOfGroup(int group)
        {
            return (this.groups.Contains(group));
        }
    }

    private struct TeleportInfo
    {
        public Vector3 location;
        public Quaternion rotation;

        public TeleportInfo(Vector3 location, Quaternion rotation)
        {
            this.location = location;
            this.rotation = rotation;
        }
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

    public override void Spawned()
    {
        Debug.Log("Spawned");
        networkRunner = FindFirstObjectByType<NetworkRunner>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!networkRunner.IsServer) return;
        CheckTPQueue();
        if (removePlayers)
        {
            removePlayers = false;
            RemoveLeftPlayers();
        }
    }

    private void Update()
    {
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
        playerObject.GetComponent<Player>().owner = player;
        playerObjects.Add(player, playerObject);
        // Add OnInstantiate later
    }

    public GameObject SpawnPlayerAtTransform(NetworkRunner runner, PlayerRef player, Transform transform)
    {
        NetworkObject playerObject = runner.Spawn(playerPrefab, transform.position, Quaternion.identity, player);
        playerObject.GetComponent<Player>().owner = player;
        playerObjects.Add(player, playerObject);
        if (playerObject == null) return null;
        return playerObject.gameObject;
    }

    public void RemoveLeftPlayers()
    {
        List<PlayerRef> removedPlayers = new List<PlayerRef>();
        foreach (KeyValuePair<PlayerRef, NetworkId> kvp in playerObjects)
        {
            if (!Runner.ActivePlayers.ToList().Contains(kvp.Key)) // active players doesnt contain the player object
            {
                removedPlayers.Add(kvp.Key);
            }
        }
        foreach (PlayerRef player in removedPlayers) playerObjects.Remove(player);
    }

    public void RemovePlayer(PlayerRef player)
    {
        // Make this less spaghetti later (by attaching removal to removeplayers variable and accessing playerobjects dict through that)
        NetworkObject obj = null;
        NetworkObject[] playerNObjects = Resources.FindObjectsOfTypeAll(typeof(NetworkObject)) as NetworkObject[];
        foreach (NetworkObject nObj in playerNObjects)
        {
            Player playerComponent = nObj.GetComponent<Player>();
            if (playerComponent == null) continue;
            if (playerComponent.owner == player)
            {
                obj = nObj;
                break;
            }
        }
        if (obj == null) return;
        NetworkObject gizmoObj = obj.GetComponent<PlayerDropManager>().gizmo.GetComponent<NetworkObject>();
        if (gizmoObj != null) networkRunner.Despawn(gizmoObj); // Removes the item gizmo
        networkRunner.Despawn(obj);
        removePlayers = true; // So that its modified only on fixedupdatenetwork
        //playerObjects.Remove(player);
        // Removes the player from observable dictionary if they are observing something
        if (playerObservables.ContainsKey(player))
        {
            GetPlayerNetworkObject(player).GetComponent<Player>().inf.SetCanInteract(true);
            playerObservables.Remove(player);
            Object.AssignInputAuthority(PlayerRef.None);
            playerObservables[player].currentPlayer = PlayerRef.None;
        }
    }

    /// <summary>
    /// Teleports the specified player to the location with the rotation
    /// </summary>
    /// <param name="player"></param>
    /// <param name="location"></param>
    /// <param name="rotation"></param>
    public void Teleport(PlayerRef player, Vector3 location, Quaternion rotation)
    {
        TeleportInfo tpInfo = new TeleportInfo(location, rotation);
        if (teleportQueue.ContainsKey(player))
        {
            teleportQueue[player] = tpInfo;
        }
        else
        {
            teleportQueue.Add(player, tpInfo);
        }
    }

    /// <summary>
    /// Checks the TP queue for players
    /// </summary>
    private void CheckTPQueue()
    {
        foreach (KeyValuePair<PlayerRef, TeleportInfo> kvp in  teleportQueue)
        {
            MovePlayer(kvp.Key, kvp.Value);
        }
        // Removes all players in teleport queue
        teleportQueue.Clear();
    }

    private void MovePlayer(PlayerRef player, TeleportInfo tpInfo)
    {
        Vector3 location = tpInfo.location;
        Quaternion rotation = tpInfo.rotation;
        if (!Runner.IsServer) return;
        GameObject tpPlayer = GetPlayerObject(player);
        Rigidbody rb = tpPlayer.GetComponent<Rigidbody>();
        PlayerMovement pm = tpPlayer.GetComponent<PlayerMovement>();
        CameraMovement cm = FindFirstObjectByType<CameraMovement>();
        rb.position = location;
        rb.rotation = rotation;
        tpPlayer.transform.position = location;
        tpPlayer.transform.rotation = rotation;
        pm.cameraPosition.eulerAngles = new Vector3 (0, rotation.eulerAngles.y, 0);
        rb.velocity = Vector3.zero;
        cm.yRotation = rotation.eulerAngles.y;
        cm.xRotation = rotation.eulerAngles.x;
        OnTeleportPlayer?.Invoke();
    }

    public void AddPlayerToGroup(PlayerRef player, int group)
    {
        if (playerProperties[player].groups == null) playerProperties[player].groups = new List<int>();
        if (playerProperties[player].groups.Contains(group)) return;
        playerProperties[player].groups.Add(group);
    }

    public void RemovePlayerFromGroup(PlayerRef player, int group)
    {
        if (playerProperties[player].groups == null) playerProperties[player].groups = new List<int>();
        if (!playerProperties[player].groups.Contains(group)) return;
        playerProperties[player].groups.RemoveAt(playerProperties[player].groups.IndexOf(group));
    }

    public List<PlayerRef> GetPlayersInGroup(int group)
    {
        List<PlayerRef> output = new List<PlayerRef>();
        foreach (KeyValuePair<PlayerRef, PlayerProperties> kvp in playerProperties)
        {
            if (kvp.Value.IsPartOfGroup(group))
            {
                output.Add(kvp.Key);
            }
        }
        return output;
    }

    public GameObject GetPlayerObject(PlayerRef player)
    {
        if (!playerObjects.ContainsKey(player)) return null;
        NetworkObject nObj;
        bool foundObject = Runner.TryFindObject(playerObjects[player], out nObj);
        if (!foundObject) return null;
        return nObj.gameObject;
    }

    public NetworkObject GetPlayerNetworkObject(PlayerRef player)
    {
        if (!playerObjects.ContainsKey(player)) return null;
        NetworkObject nObj;
        bool foundObject = Runner.TryFindObject(playerObjects[player], out nObj);
        if (!foundObject) return null;
        return nObj;
    }
}
