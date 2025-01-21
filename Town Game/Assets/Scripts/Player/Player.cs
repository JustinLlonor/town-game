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
    PlayerManager playerManager;
    // To sync the inputs on all other clients
    [Networked] float camDirection { get; set; }
    [Networked] float camDirectionX { get; set; }
    [Networked] Vector2 direction { get; set; }
    [Networked] Vector3 networkPosition { get; set; }
    [Networked] int tick { get; set; }

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

    private void Update()
    {
        if (IsProxy)
        {
            Prediction();
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
        }
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
    }
}
