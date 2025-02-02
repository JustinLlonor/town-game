using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner nRunner;
    [Header("Input")]
    public int waitingRoomIndex = 1;
    public Vector2 moveDirection;
    public float orientation;
    public float camOrientation;
    public bool jump = false;
    public bool crouch = false;
    public bool sprint = false;
    public bool menu = false;
    public int hotbarKey = 1;
    public bool interactionPressed = false;
    public int interactionKey = 0;

    public async void StartGame(GameMode mode, string name, int sceneIndex)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        nRunner = gameObject.AddComponent<NetworkRunner>();
        RunnerSimulatePhysics3D pSim = gameObject.AddComponent<RunnerSimulatePhysics3D>();
        pSim.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;
        nRunner.ProvideInput = true;

        SteamInviteAccepter sia = FindObjectOfType<SteamInviteAccepter>();
        if (sia != null) sia.DisableInvites();

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(sceneIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Single);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await nRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = name,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    // Finds the player manager and spawns the player
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            FindObjectOfType<PlayerManager>().SpawnPlayer(runner, player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // Movement
        data.direction = moveDirection;
        data.camDirection = orientation;
        data.camDirectionX = camOrientation;
        data.buttons.Set(NetworkInputData.Buttons.Jump, jump);
        jump = false;
        data.buttons.Set(NetworkInputData.Buttons.Crouch, crouch);
        data.buttons.Set(NetworkInputData.Buttons.Sprint, sprint);
        // Hotbar
        data.hotbarKey = hotbarKey;
        hotbarKey = 0;
        // Menu
        data.menu = menu;
        // Interactables
        data.interactPressed = interactionPressed;
        data.interaction = interactionKey;

        input.Set(data);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (nRunner.LocalPlayer == player) SteamMatchmaking.LeaveLobby((CSteamID)SessionData.steamIdLobby); // Leaves steam lobby
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
