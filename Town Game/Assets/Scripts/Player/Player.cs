using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Player : NetworkBehaviour
{
    public delegate void PlayerEvent();
    public PlayerEvent Init;
    public int simulationTickDistance = 2;
    [HideInInspector] public PlayerMovement pm;
    Transform playerGFX;
    Transform cameraPosition;
    PlayerManager playerManager;
    NetworkObject no;
    // To sync the inputs on all other clients
    [Networked] float camDirection { get; set; }
    [Networked] float camDirectionX { get; set; }
    [Networked] Vector2 direction { get; set; }
    float timer = 0f;
    int simCount = 0;

    private void Awake()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        no = GetComponent<NetworkObject>();
        playerGFX = pm.graphics;
        cameraPosition = pm.cameraPosition;
    }

    private void Start()
    {
        playerManager.SetupMovementSettings(gameObject);
        if (!HasInputAuthority) return;
        playerManager.SetupOnClient(gameObject);
    }

    private void Update()
    {
        if (IsProxy)
        {
            Prediction();
        }

        if (!HasInputAuthority) return;

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
    }

    public override void FixedUpdateNetwork()
    {
        //if (!HasInputAuthority && Runner.IsServer)
        //{
        //    networkPosition = transform.position;
        //    tick = 
        //}
        if (GetInput(out NetworkInputData data))
        {
            pm.horizontalMovement = data.direction.X;
            pm.verticalMovement = data.direction.Y;
            if (!HasInputAuthority)
            {
                playerGFX.rotation = Quaternion.Euler(0f, data.camDirection, 0f);
                pm.orientation.rotation = Quaternion.Euler(0f, data.camDirection, 0f);
                //pm.SetCamRotation(data.camDirectionX);
                cameraPosition.rotation = Quaternion.Euler(camDirectionX, camDirection, 0f);
                //headAim.localRotation = Quaternion.Euler(camDirectionX, 0f, 0f);
            }
            if (HasStateAuthority)
            {
                camDirection = data.camDirection;
                camDirectionX = data.camDirectionX;
                direction = data.direction;
            }
            Simulate();
        }
    }

    private void Simulate()
    {
        pm.Inputs();
        pm.MovePlayer();
        pm.CapAirVelocity();
        pm.StepClimb();
    }

    //Prediction for the player movement, executed on proxies
    private void Prediction()
    {
        playerGFX.rotation = Quaternion.Euler(0f, camDirection, 0f);
        cameraPosition.rotation = Quaternion.Euler(camDirectionX, camDirection, 0f);
        pm.SetDirection(direction);
    }
    
    public override void Spawned()
    {
        Init?.Invoke();
        Runner.SetPlayerObject(Runner.LocalPlayer, Object);
    }
}
