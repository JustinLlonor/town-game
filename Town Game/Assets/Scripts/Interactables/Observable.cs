using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observable : MonoBehaviour
{
    public Transform observeCameraTransform;
    public float transitionDuration = 0.5f;
    public float give = 20f;
    bool isObserving;
    bool transitioning = false;
    CameraManager cm;

    private void Awake()
    {
        cm = FindFirstObjectByType<CameraManager>();
    }

    /// <summary>
    /// Starts the observation for this observable
    /// </summary>
    public void StartObservation()
    {
        if (isObserving) return;
        if (transitioning) return;
        if (cm.mode != CameraManager.CameraMode.FirstPerson) return;
        cm.trackedObservableTransform = observeCameraTransform;
        cm.StartModeTransition(transitionDuration, CameraManager.CameraMode.Observe);
        WaitTransition(); // Make player stuff here, network player states like isMoving
        isObserving = true;
    }
    
    /// <summary>
    /// Stops the observation for this observable
    /// </summary>
    public void ExitObservation()
    {
        if (!isObserving) return;
        if (transitioning) return;
        cm.StartFPSTransition(transitionDuration);
        WaitTransition();
        isObserving = false;
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
