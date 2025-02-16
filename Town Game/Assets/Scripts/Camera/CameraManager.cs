using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;
    public Camera uiFront;
    public CameraMode mode;
    public Transform trackedFPSTransform;
    public Transform trackedCinematicTransform;
    public Transform trackedObservableTransform;
    [SerializeField] private Observable currentObservable;

    public SwitchCameraMode OnSwitchCameraMode;
    public delegate void SwitchCameraMode(CameraMode mode);

    public bool isTransitioning = false;

    public enum CameraMode
    {
        FirstPerson = 0,
        Cinematic = 1,
        Observe = 2
    }

    public Observable GetCurrentObservable() { return currentObservable; }

    public void ChangeCameraMode(CameraMode newMode)
    {
        mode = newMode;
        if (newMode == CameraMode.FirstPerson)
        {
            transform.rotation = trackedFPSTransform.rotation;
        }
        if (newMode == CameraMode.Cinematic)
        {
            if (trackedCinematicTransform != null) transform.position = trackedCinematicTransform.position;
        }
        if (newMode == CameraMode.Observe)
        {
            if (trackedObservableTransform != null)
            {
                transform.position = trackedObservableTransform.position;
                transform.rotation = trackedObservableTransform.rotation;
            }
        }
        OnSwitchCameraMode?.Invoke(mode);
    }

    /// <summary>
    /// Sets the transform the camera is tracking during fps mode
    /// </summary>
    /// <param name="transform"></param>
    public void SetTrackedFPSTransform(Transform transform)
    {
        trackedFPSTransform = transform;
    }

    public void SetTrackedCinematicTransform(Transform transform)
    {
        trackedCinematicTransform = transform;
    }

    void Update()
    {
        if (isTransitioning) return;
        if (mode == CameraMode.FirstPerson)
        {
            if (trackedFPSTransform != null) transform.position = trackedFPSTransform.position;
        }
        if (mode == CameraMode.Cinematic)
        {
            if (trackedCinematicTransform != null)
            {
                //Set to cinematic rotation and position
                transform.position = trackedCinematicTransform.position;
                transform.rotation = trackedCinematicTransform.rotation;
            }
        }
        if (mode == CameraMode.Observe)
        {
            if (trackedObservableTransform != null)
            {
                transform.position = trackedObservableTransform.position;
                transform.rotation = trackedObservableTransform.rotation;
            }
        }
    }

    public void StartFPSTransition(float duration)
    {
        if (mode == CameraMode.FirstPerson) return;
        StopAllCoroutines();
        StartCoroutine(TransitionToMode(duration, CameraMode.FirstPerson));
    }

    // Starts a new mode transition, smoothly interpolates the camera to the corresponding mode transform
    public void StartModeTransition(float duration, CameraMode newMode)
    {
        if (mode == newMode) return;
        if (newMode != CameraMode.FirstPerson) trackedFPSTransform.rotation = transform.rotation;
        StopAllCoroutines();
        StartCoroutine(TransitionToMode(duration, newMode));
    }

    IEnumerator TransitionToMode(float duration, CameraMode newMode)
    {
        isTransitioning = true;
        float time = 0f;
        Vector3 ogPos = transform.position;
        Quaternion ogRot = transform.rotation;
        Transform newTransform = null;
        if (newMode == CameraMode.FirstPerson) newTransform = trackedFPSTransform;
        if (newMode == CameraMode.Cinematic) newTransform = trackedCinematicTransform;
        if (newMode == CameraMode.Observe) newTransform = trackedObservableTransform;
        
        while (time < 1f)
        {
            time += Time.deltaTime * (1/duration);

            transform.position = Vector3.Lerp(ogPos, newTransform.position, Mathf.SmoothStep(0f, 1f, time));
            transform.rotation = Quaternion.Lerp(ogRot, newTransform.rotation, Mathf.SmoothStep(0f, 1f, time));

            yield return null;
        }

        isTransitioning = false;
        ChangeCameraMode(newMode);
    }
}
