using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Player : NetworkBehaviour
{
    public delegate void PlayerEvent();
    public PlayerEvent Init;
    [HideInInspector] public PlayerMovement pm;
    PlayerManager playerManager;
    // To sync the inputs on all other clients
    [Networked] float camDirection { get; set; }
    [Networked] float camDirectionX { get; set; }
    [Networked] Vector2 direction { get; set; }

    Rigidbody rb;

    private void Awake()
    {
        playerManager = FindObjectOfType<PlayerManager>();   
        rb = GetComponent<Rigidbody>();
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
                rb.rotation = Quaternion.Euler(0f, data.camDirection, 0f);
                pm.SetCamRotation(data.camDirectionX);
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

    private void Prediction()
    {
        //pm.horizontalMovement = direction.x;
        //pm.verticalMovement = direction.y;
        //transform.rotation = Quaternion.Euler(0f, camDirection, 0f);
        rb.rotation = (Quaternion.Euler(0f, camDirection, 0f));
        pm.SetDirection(direction);
        return;
        pm.Inputs();
        pm.MovePlayer();
        pm.CapAirVelocity();
        pm.StepClimb();
    }

    public override void Spawned()
    {
        Init?.Invoke();
    }
}
