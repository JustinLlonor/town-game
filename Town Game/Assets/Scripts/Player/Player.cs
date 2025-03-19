using Fusion;
using Fusion.Addons.Physics;
using Steamworks;
using UnityEngine;
using WebSocketSharp;

public class Player : NetworkBehaviour
{
    [Networked] public string nickname { get; set; } = "";
    public delegate void PlayerEvent();
    public PlayerEvent Init;
    public LayerMask glitchLayer;
    public GameObject serverItem;
    [HideInInspector] public PlayerMovement pm;
    [HideInInspector] public PlayerInventory pi;
    [HideInInspector] public PlayerDropManager dropManager;
    [HideInInspector] public InteractableFinder inf;
    [HideInInspector] public ItemUse itemUse;
    [HideInInspector] public PlayerClothing playerClothing;
    Transform playerGFX;
    Transform cameraPosition;
    PlayerManager playerManager;
    RunnerManager rm;
    CameraManager cm;
    // To sync the inputs on all other clients
    [Networked] public float camDirection { get; set; }
    [Networked] public float camDirectionX { get; set; }
    [Networked] Vector2 direction { get; set; }
    bool nicknameSet = false;
    bool previousCrouchSet = false;

    private void Awake()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
        playerGFX = pm.graphics;
        cameraPosition = pm.cameraPosition;
    }

    private void Start()
    {
        playerManager.SetupMovementSettings(gameObject);
        if (!HasInputAuthority) return;
        playerManager.SetupOnClient(gameObject);
    }

    public override void Spawned()
    {
        cm = FindFirstObjectByType<CameraManager>();
        rm = FindFirstObjectByType<RunnerManager>();
        Init?.Invoke();
        if (!HasInputAuthority) return;
        if (SteamManager.Initialized) RPC_SendNickname(SteamFriends.GetPersonaName());
        else RPC_SendNickname("Player " + Object.InputAuthority.PlayerId.ToString());
        UIManager.instance.OnUIOpen += MenuOpen;
        UIManager.instance.OnUIClose += MenuClose;
        InputManager inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onExitObserve += OnExitObservable;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            pm.horizontalMovement = data.direction.X; // Horizontal and vertical movement inputs
            pm.verticalMovement = data.direction.Y;
            if (!HasInputAuthority) // Syncs player rotation, rotates player models
            {
                playerGFX.rotation = Quaternion.Euler(0f, data.camDirection, 0f);
                pm.orientation.rotation = Quaternion.Euler(0f, data.camDirection, 0f); // Orientation transform points toward the direction the player moves in
                cameraPosition.rotation = Quaternion.Euler(camDirectionX, camDirection, 0f);
            }
            if (HasStateAuthority) // Sets properties
            {
                camDirection = data.camDirection;
                camDirectionX = data.camDirectionX;
                direction = data.direction;
            }
            // Player movement
            if (data.buttons.IsSet(NetworkInputData.Buttons.Jump))
            {
                pm.Jump();
            }
            CrouchSet(data.buttons.IsSet(NetworkInputData.Buttons.Crouch)); // Crouching stuff, executes functions on first press
            pm.sprintPressed = data.buttons.IsSet(NetworkInputData.Buttons.Sprint);
            // Player inventory
            if (!(data.hotbarKey <= 0))
            {
                PlayerInventory(data.hotbarKey);
            }
            // Interactable Finder
            inf.menuData = data.menu;
            inf.forwardDirection = Quaternion.Euler(data.camDirectionX, data.camDirection, 0f) * Vector3.forward; // orientation/camDirection is mouse x
            inf.currentKey = (Interactable.InteractKey)data.interaction;
            inf.currentPressed = data.interactPressed;
            // Dropping
            dropManager.dropPressed = data.buttons.IsSet(NetworkInputData.Buttons.Drop);
            // Observables
            if (data.buttons.IsSet(NetworkInputData.Buttons.ExitObserve))
            {
                ExitObserve();
            }
            if (data.subInteractableIndex != -1 && !Runner.IsResimulation)
            {
                IncreaseSIAtIndex(data.subInteractableIndex);
            }
            // Items
            if (data.buttons.IsSet(NetworkInputData.Buttons.PrimaryItem))
            {
                itemUse.UseItem();
            }
            if (data.buttons.IsSet(NetworkInputData.Buttons.SecondaryItem))
            {
                itemUse.UseSecondary();
            }
        }
        Simulate();
    }

    private void IncreaseSIAtIndex(int index)
    {
        ItemObservable io = null;
        if (HasStateAuthority)
        {
            if (!playerManager.playerObservables.ContainsKey(Object.InputAuthority)) return;
            if (!(playerManager.playerObservables[Object.InputAuthority] is ItemObservable)) return;
            io = (ItemObservable)playerManager.playerObservables[Object.InputAuthority];
        }
        if (HasInputAuthority)
        {
            io = (ItemObservable)cm.GetCurrentObservable();
        }

        io.IncreaseSIProgress(Runner.DeltaTime, index);
    }

    // An RPC sent from the player to the server to set the nickname
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendNickname(string name)
    {
        if (nicknameSet) return;
        nickname = name;
        nicknameSet = true;
    }

    private void Update()
    {
        if (IsProxy)
        {
            Prediction();
        }
    }

    void MenuOpen()
    {
        rm.menu = true;
    }

    void MenuClose()
    {
        rm.menu = false;
    }

    private void OnExitObservable()
    {
        rm.exitObservePressed = true;
    }

    private void ExitObserve()
    {
        if (Runner.IsServer)
        {
            PlayerRef thisPlayer = Object.InputAuthority;
            if (playerManager.playerObservables.ContainsKey(thisPlayer))
            {
                playerManager.playerObservables[thisPlayer].ExitObservationNetwork(thisPlayer);
            }
        }
        if (HasInputAuthority)
        {
            CameraManager cameraManager = FindFirstObjectByType<CameraManager>();
            cameraManager.GetCurrentObservable().ExitObservation();
        }
    }

    void CrouchSet(bool crouchPressed)
    {
        if (!crouchPressed) pm.ExitCrouch();
        if (crouchPressed == previousCrouchSet) return; // If they don't need to change, return
        previousCrouchSet = crouchPressed;
        if (crouchPressed)
        {
            pm.EnterCrouch();
        }
    }

    private void PlayerInventory(int slot)
    {
        if (!pi.hotbar[pi.equippedSlot].ToString().IsNullOrEmpty())
        {
            // if (pi.equippedItem.large) return; Do later, if the equipped item is large then return
        }
        pi.EquipItem(slot - 1);
        if (HasInputAuthority) pi.UpdateHotbarUI();
    }

    private void Simulate()
    {
        pm.SetIsMoving();
        pm.Inputs();
        pm.MovePlayer();
        pm.CapAirVelocity();
        pm.StepClimb();
        pm.GroundSim();
    }

    //Prediction for the player movement, executed on proxies
    private void Prediction()
    {
        playerGFX.rotation = Quaternion.Euler(0f, camDirection, 0f);
        cameraPosition.rotation = Quaternion.Euler(camDirectionX, camDirection, 0f);
        pm.SetDirection(direction);
    }

    public void EnableUIFront()
    {
        playerClothing.SetClothingLayer((int)Mathf.Log(glitchLayer.value, 2));
        serverItem.layer = (int)Mathf.Log(glitchLayer.value, 2);
    }

    public void DisableUIFront()
    {
        playerClothing.SetClothingLayer(0);
        serverItem.layer = 0;
    }
}
