using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;
    public Camera uiFront;
    public CameraMode mode;
    public Rigidbody trackedFPSRigidbody;
    public Transform trackedFPSTransform;
    public CameraLevel trackedCamLevel;
    public Transform trackedCinematicTransform;
    public Transform trackedObservableTransform;
    public CameraMovement cm;
    [SerializeField] private Observable currentObservable;
    UIManager uiManager;

    /// <summary>
    /// Called when the camera switches modes
    /// </summary>
    public CameraModeEvent onSwitchCameraMode;
    /// <summary>
    /// Called when a camera transition starts
    /// </summary>
    public CameraModeEvent onStartCameraTransition;
    public delegate void CameraModeEvent(CameraMode mode);

    public bool isTransitioning = false;

    public enum CameraMode
    {
        FirstPerson = 0,
        Cinematic = 1,
        Observe = 2
    }

    private void Awake()
    {
        uiManager = FindAnyObjectByType<UIManager>();
    }

    public void SetCurrentObservable(Observable observable)
    {
        currentObservable = observable;
    }

    public Observable GetCurrentObservable() { return currentObservable; }

    public void ChangeCameraMode(CameraMode newMode)
    {
        mode = newMode;
        if (newMode == CameraMode.FirstPerson)
        {
            transform.rotation = trackedFPSRigidbody.rotation;
        }
        if (newMode == CameraMode.Cinematic)
        {
            uiManager.ExitUI();
            if (trackedCinematicTransform != null) transform.position = trackedCinematicTransform.position;
        }
        if (newMode == CameraMode.Observe)
        {
            if (trackedObservableTransform != null)
            {
                transform.position = trackedObservableTransform.position;
                transform.rotation = trackedObservableTransform.rotation;
            }
        } else
        {
            currentObservable = null;
        }
        onSwitchCameraMode?.Invoke(mode);
    }

    /// <summary>
    /// Sets the transform and rigidbody the camera is tracking during fps mode
    /// </summary>
    /// <param name="rb"></param>
    public void SetTrackedFPS(Rigidbody rb, Transform trackedTransform, CameraLevel camLevel)
    {
        trackedFPSRigidbody = rb;
        trackedFPSTransform = trackedTransform;
        trackedCamLevel = camLevel;
    }

    public void SetTrackedCinematicTransform(Transform transform)
    {
        trackedCinematicTransform = transform;
    }

    void LateUpdate()
    {
        SetPositions();
    }

    public void SetPositions()
    {
        if (isTransitioning) return;
        if (mode == CameraMode.FirstPerson)
        {
            if (trackedFPSRigidbody != null)
            {
                transform.position = new Vector3(trackedFPSRigidbody.position.x, trackedFPSRigidbody.position.y + trackedCamLevel.yLevel, 
                    trackedFPSRigidbody.position.z);
            }
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
        onStartCameraTransition?.Invoke(CameraMode.FirstPerson);
        StopAllCoroutines();
        StartCoroutine(TransitionToMode(duration, CameraMode.FirstPerson));
    }

    // Starts a new mode transition, smoothly interpolates the camera to the corresponding mode transform
    public void StartModeTransition(float duration, CameraMode newMode)
    {
        if (mode == newMode) return;
        if (newMode != CameraMode.FirstPerson) trackedFPSRigidbody.rotation = transform.rotation;
        onStartCameraTransition?.Invoke(newMode);
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
