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
    [HideInInspector] public PlayerMovement pm;
    Transform playerGFX;
    Transform cameraPosition;
    Transform headAim;
    PlayerManager playerManager;
    // To sync the inputs on all other clients
    [Networked] float camDirection { get; set; }
    [Networked] float camDirectionX { get; set; }
    [Networked] Vector2 direction { get; set; }

    NetworkRigidbody3D rb;

    private void Awake()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        playerGFX = pm.graphics;
        cameraPosition = pm.cameraPosition;
        headAim = pm.headAim;
    }

    private void Start()
    {
        if (!HasInputAuthority) return;
        playerManager.SetupOnClient(gameObject);
    }

    private void Update()
    {
        if (IsProxy)
        {
            Prediction();
        }
    }

    public override void FixedUpdateNetwork()
    {
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
            pm.Inputs();
            pm.MovePlayer();
            pm.CapAirVelocity();
            pm.StepClimb();
        }
    }

    //Prediction for the player movement, executed on proxies
    private void Prediction()
    {
        //pm.horizontalMovement = direction.x;
        //pm.verticalMovement = direction.y;
        //transform.rotation = Quaternion.Euler(0f, camDirection, 0f);
        playerGFX.rotation = Quaternion.Euler(0f, camDirection, 0f);
        cameraPosition.rotation = Quaternion.Euler(camDirectionX, camDirection, 0f);
        pm.SetDirection(direction);
        //headAim.localRotation = Quaternion.Euler(camDirectionX, 0f, 0f);
        //pm.Inputs();
        //pm.MovePlayer();
        //pm.CapAirVelocity();
        //pm.StepClimb();
    }

    public override void Spawned()
    {
        Init?.Invoke();
    }
}
