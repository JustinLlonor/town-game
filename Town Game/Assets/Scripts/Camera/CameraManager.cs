using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CameraMode mode;
    public Transform trackedFPSTransform;
    public Transform trackedCinematicTransform;

    public SwitchCameraMode OnSwitchCameraMode;
    public delegate void SwitchCameraMode(CameraMode mode);

    bool isTransitioning = false;

    public enum CameraMode
    {
        FirstPerson = 0,
        Cinematic = 1,
    }

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
                transform.position = trackedCinematicTransform.position;
                transform.rotation = trackedCinematicTransform.rotation;
            }
        }
    }

    public void StartFPSTransition(float duration)
    {
        if (mode == CameraMode.FirstPerson) return;
        StopAllCoroutines();
        StartCoroutine(TransitionToFPS(duration));
    }

    IEnumerator TransitionToFPS(float duration)
    {
        isTransitioning = true;
        float time = 0f;
        Vector3 ogPos = transform.position;
        Quaternion ogRot = transform.rotation;
        
        while (time < 1f)
        {
            time += Time.deltaTime * (1/duration);

            transform.position = Vector3.Lerp(ogPos, trackedFPSTransform.position, Mathf.SmoothStep(0f, 1f, time));
            transform.rotation = Quaternion.Lerp(ogRot, trackedFPSTransform.rotation, Mathf.SmoothStep(0f, 1f, time));

            yield return null;
        }

        ChangeCameraMode(CameraMode.FirstPerson);
        isTransitioning = false;
    }
}
