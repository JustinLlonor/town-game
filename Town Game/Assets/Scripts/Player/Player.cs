using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Player : NetworkBehaviour
{
    [Networked] public string nickname { get; set; } = "";
    public delegate void PlayerEvent();
    public PlayerEvent Init;
    public int simulationTickDistance = 2;
    [HideInInspector] public PlayerMovement pm;
    Transform playerGFX;
    Transform cameraPosition;
    PlayerManager playerManager;
    // To sync the inputs on all other clients
    [Networked] public float camDirection { get; set; }
    [Networked] float camDirectionX { get; set; }
    [Networked] Vector2 direction { get; set; }

    private void Awake()
    {
        playerManager = FindObjectOfType<PlayerManager>();
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
        if (HasInputAuthority) RPC_SendNickname(SessionData.nickname);
        Init?.Invoke();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendNickname(string name)
    {
        nickname = name;
    }

    private void Update()
    {
        if (IsProxy)
        {
            Prediction();
        }

        if (!HasInputAuthority) return;
        /**
        if (timer <= 0f)
        {
            Debug.LogError(simCount);
            timer = 1f;
            simCount = 0;
        } 
        else
        {
            timer -= Time.deltaTime;
        }
        **/
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
            if (data.buttons.IsSet(NetworkInputData.Buttons.Jump))
            {
                pm.Jump();
            }
            if (data.buttons.IsSet(NetworkInputData.Buttons.Crouch))
            {
                pm.EnterCrouch();
            }
            else
            {
                pm.ExitCrouch();
            }
            pm.sprintPressed = data.buttons.IsSet(NetworkInputData.Buttons.Sprint);
        }
        Simulate();
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
}
