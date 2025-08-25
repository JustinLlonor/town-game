using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner nRunner;
    public int mainMenuIndex = 0;
    public int waitingRoomIndex = 1;
    public int testRoomIndex = 3;
    [Header("Input")]
    public Vector2 moveDirection;
    public float orientation;
    public float camOrientation;
    public bool jump = false;
    public bool crouch = false;
    public bool sprint = false;
    public bool menu = false;
    public int hotbarKey = 1;
    public bool interactionPressed = false;
    public int interactIndex = 0;
    public bool dropPressed = false;
    public bool exitObservePressed = false;
    public int siPressed = -1;
    public bool itemUsePrimary;
    public bool itemUseSecondary;
    public bool rotateModePressed = false;
    public float lockedDelta = 0f;
    [HideInInspector] public bool heldOnSI = false;
    [HideInInspector] public bool isHoldSI;
    public bool firstFrame = true;
    public Recorder recorder;

    public PlayerEvent onPlayerJoin;
    public PlayerEvent onPlayerLeave;
    public delegate void PlayerEvent(PlayerRef player);

    PlayerManager pm;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetInputs();
    }


    /// <summary>
    /// Resets the inputs for this client, call 
    /// </summary>
    public void ResetInputs()
    {
        moveDirection = Vector2.zero;
        orientation = 0f;
        camOrientation = 0f;
        jump = false;
        crouch = false;
        sprint = false;
        menu = false;
        hotbarKey = 1;
        interactionPressed = false;
        interactIndex = -1;
        dropPressed = false;
        exitObservePressed = false;
        siPressed = -1;
        itemUsePrimary = false;
        itemUseSecondary = false;
        heldOnSI = false;
        isHoldSI = false;
        firstFrame = true;
        rotateModePressed = false;
        lockedDelta = 0f;
    }

    public async void StartGame(GameMode mode, string name, int sceneIndex)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        RunnerSimulatePhysics3D pSim = gameObject.AddComponent<RunnerSimulatePhysics3D>();
        pSim.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;
        nRunner.ProvideInput = true;

        SteamInviteAccepter sia = FindFirstObjectByType<SteamInviteAccepter>();
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

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        onPlayerJoin?.Invoke(player);
        if (runner.IsServer)
        {
            pm = FindFirstObjectByType<PlayerManager>();
            if (pm.spawnPlayersOnJoin) pm.SpawnPlayer(runner, player); // Adds a new player when a player joins
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        onPlayerLeave?.Invoke(player);
        if (nRunner.LocalPlayer == player) SteamMatchmaking.LeaveLobby((CSteamID)SessionData.steamIdLobby); // Leaves steam lobby
        if (runner.IsServer)
        {
            pm.RemovePlayer(player);
            //pm.RemovePlayer(player); // annihalates the palyer
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
        data.buttons.Set(NetworkInputData.Buttons.Drop, dropPressed);
        // Hotbar
        data.hotbarKey = hotbarKey;
        hotbarKey = 0;
        // Menu
        data.menu = menu;
        // Interactables
        data.interactPressed = interactionPressed;
        data.interaction = interactIndex;
        data.buttons.Set(NetworkInputData.Buttons.ExitObserve, exitObservePressed);
        exitObservePressed = false;
        if (siPressed != -1)
        {
            if (isHoldSI)
            {
                firstFrame = false;
                if (heldOnSI)
                {
                    data.subInteractableIndex = siPressed;
                } else
                {
                    data.subInteractableIndex = -1;
                }
            }
            else
            {
                if (firstFrame && heldOnSI)
                {
                    data.subInteractableIndex = siPressed;
                    firstFrame = false;
                }
                else
                {
                    data.subInteractableIndex = -1;
                }
            }
        } else
        {
            firstFrame = true;
            data.subInteractableIndex = -1;
        }
        // Item use
        data.itemUsePrimary = itemUsePrimary;
        data.itemUseSecondary = itemUseSecondary;
        //data.buttons.Set(NetworkInputData.Buttons.PrimaryItem, itemUsePrimary);
        //data.buttons.Set(NetworkInputData.Buttons.SecondaryItem, itemUseSecondary);
        data.rotateModePressed = rotateModePressed;
        data.rotateDelta = lockedDelta;

        input.Set(data);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        SceneManager.LoadScene(mainMenuIndex);
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
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
