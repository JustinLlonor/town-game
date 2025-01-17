using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public delegate void PlayerEvent();
    public PlayerEvent Init;
    [HideInInspector] public PlayerMovement pm;
    PlayerManager playerManager;

    private void Awake()
    {
        playerManager = FindObjectOfType<PlayerManager>();   
    }

    private void Start()
    {
        if (!HasInputAuthority) return;
        playerManager.SetupOnClient(gameObject);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            pm.horizontalMovement = data.direction.X;
            pm.verticalMovement = data.direction.Y;
            if (!HasStateAuthority) pm.orientation.rotation = Quaternion.Euler(0f, data.camDirection, 0f);
            pm.Inputs();
            pm.MovePlayer();
            pm.CapAirVelocity();
            pm.StepClimb();
        }
    }

    public override void Spawned()
    {
        Init?.Invoke();
    }
}
