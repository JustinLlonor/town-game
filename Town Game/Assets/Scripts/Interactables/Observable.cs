using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Observable : NetworkBehaviour
{
    [Networked] public PlayerRef currentPlayer { get; set; }
    public Transform observeCameraTransform;
    public float transitionDuration = 0.5f;
    public bool networked = true;
    bool isObserving; // Client side
    bool transitioning = false; // Client side
    CameraManager cm;
    NetworkRunner runner;

    public ObservationPlayerEvent onStartObservation;
    public delegate void ObservationPlayerEvent(PlayerRef player);

    private void Awake()
    {
        cm = FindFirstObjectByType<CameraManager>();
        runner = FindFirstObjectByType<NetworkRunner>();
    }

    private void Update()
    {
        UpdateObservationCheck();
    }

    /// <summary>
    /// Exists if there is another player using the observable
    /// </summary>
    void UpdateObservationCheck()
    {
        if (!isObserving) return;
        if (CheckObservationTaken(runner.LocalPlayer))
        {
            ExitObservation(false);
        }
    }

    /// <summary>
    /// Checks if the current observation is taken
    /// </summary>
    /// <returns>Returrns true if there is another player observing otherwise false.</returns>
    bool CheckObservationTaken(PlayerRef checkedPlayer)
    {
        if (currentPlayer == PlayerRef.None) return false;
        if (currentPlayer == checkedPlayer) return false;
        return true;
    }

    /// <summary>
    /// Starts the observation for this observable
    /// </summary>
    public void StartObservation()
    {
        if (isObserving) return;
        if (transitioning) return;
        if (cm.mode != CameraManager.CameraMode.FirstPerson) return;
        if (CheckObservationTaken(runner.LocalPlayer)) return;
        cm.trackedObservableTransform = observeCameraTransform;
        cm.StartModeTransition(transitionDuration, CameraManager.CameraMode.Observe);
        cm.SetCurrentObservable(this);
        StartObservationEvent();
        WaitTransition(); // Make player stuff here, network player states like isMoving
        isObserving = true;
    }
    
    /// <summary>
    /// Stops the observation for this observable
    /// </summary>
    public void ExitObservation(bool checkTransition = true)
    {
        if (!isObserving) return;
        if (transitioning && checkTransition) return;
        cm.StartFPSTransition(transitionDuration);
        WaitTransition();
        isObserving = false;
    }

    /// <summary>
    /// StartObservation is on client, while StartObservationNetwork is on server
    /// </summary>
    /// <param name="player"></param>
    public void StartObservationNetwork(PlayerRef player)
    {
        if (CheckObservationTaken(player)) return;
        currentPlayer = player;
    }

    /// <summary>
    /// To be executed with ExitObservation for the server
    /// </summary>
    public void ExitObservationNetwork(PlayerRef player)
    {
        if (currentPlayer != player) return;
        currentPlayer = PlayerRef.None;
    }

    void StartObservationEvent()
    {
        if (runner == null) return;
        onStartObservation?.Invoke(runner.LocalPlayer);
    }

    void WaitTransition()
    {
        transitioning = true;
        Invoke("SwitchTransitionBool", transitionDuration);
    }

    void SwitchTransitionBool()
    {
        transitioning = !transitioning;
    }
}
