using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    public float walkLength = 1f;
    public float sprintLength = 1f;
    public float crouchLength = 1f;
    public float wAmplitude = .1f;
    public float sAmplitude = .1f;
    public float cAmplitude = 0.04f;
    public float zTilt = 1f;
    public float resetSpeed = 5f;
    public float transitionSpeed = 5f;
    public bool isBobbing = false;
    public bool isSprinting = false;
    public bool isCrouching = false;
    public Transform fpsBob;
    public float fpsXMultiplier;
    public float fpsYMultiplier;
    float bobPosition;
    float previousLength = 1f;
    float previousAmplitude = 1f;
    float currentLength = 1f;
    float currentAmplitude = 1f;
    float currentWeight = 1f;

    private void Awake()
    {
        currentLength = walkLength;
        currentAmplitude = wAmplitude;
    }

    private void LateUpdate()
    {
        Weights();
        if (isBobbing) Bob();
        if (!isBobbing) ResetPos();
        if (!isCrouching || !isBobbing)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * resetSpeed);
        }
    }

    void Weights()
    {
        if (!isBobbing) return;
        if (isSprinting)
        {
            SetNewCurrent(sprintLength, sAmplitude);
        }
        if (!isSprinting && !isCrouching)
        {
            SetNewCurrent(walkLength, wAmplitude);
        }
        if (isCrouching)
        {
            SetNewCurrent(crouchLength, cAmplitude);
        }

        if (currentWeight != 1f)
        {
            currentWeight += Time.deltaTime * transitionSpeed;
            currentWeight = Mathf.Clamp01(currentWeight);
        }
    }

    void SetNewCurrent(float length, float amplitude)
    {
        if (length != currentLength)
        {
            currentWeight = 0f;
            previousLength = currentLength;
            previousAmplitude = currentAmplitude;
        }
        currentLength = length;
        currentAmplitude = amplitude;
    }

    void Bob()
    {
        bobPosition += Time.deltaTime;
        float yPrevious = Mathf.Sin(2 / previousLength * 2 * Mathf.PI * bobPosition) * previousAmplitude;
        float xPrevious = Mathf.Sin(1 / previousLength * 2 * Mathf.PI * bobPosition) * previousAmplitude / 2f;
        float yCurrent = Mathf.Sin(2 / currentLength * 2 * Mathf.PI * bobPosition) * currentAmplitude;
        float xCurrent = Mathf.Sin(1 / currentLength * 2 * Mathf.PI * bobPosition) * currentAmplitude / 2f;
        float y = (yCurrent - yPrevious) * currentWeight + yPrevious;
        float x = (xCurrent - xPrevious) * currentWeight + xPrevious;
        transform.localPosition = new Vector3(x, y, 0f);
        fpsBob.localPosition = new Vector3(x * fpsXMultiplier, y * fpsYMultiplier, 0f);
        if (isCrouching)
        {
            float zRot = x * zTilt;
            transform.localEulerAngles = new Vector3(transform.localRotation.x, transform.localRotation.y, zRot);
        }
    }
    
    void ResetPos()
    {
        bobPosition = 0f;
        if (transform.localPosition != Vector3.zero)
        {
            transform.localPosition = Vector3.Slerp(transform.localPosition, Vector3.zero, Time.deltaTime * resetSpeed);
        }
        if (fpsBob.localPosition != Vector3.zero)
        {
            fpsBob.localPosition = Vector3.Slerp(fpsBob.localPosition, Vector3.zero, Time.deltaTime * resetSpeed);
        }
    }
}
