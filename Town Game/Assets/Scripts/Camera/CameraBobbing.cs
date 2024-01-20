using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    public float walkLength = 1f;
    public float sprintLength = 1f;
    public float amplitude = .1f;
    public float sAmplitude = .1f;
    public float resetSpeed = 5f;
    public float transitionSpeed = 5f;
    public bool isBobbing = false;
    public bool isSprinting = false;
    float bobPosition;
    float sprintWeight = 0f;

    private void Update()
    {
        SprintWeight();
        if (isBobbing) Bob();
        if (!isBobbing) ResetPos();
    }

    void SprintWeight()
    {
        if (isSprinting && sprintWeight != 1f)
        {
            sprintWeight += Time.deltaTime * transitionSpeed;
        }
        if (!isSprinting && sprintWeight != 0f)
        {
            sprintWeight -= Time.deltaTime * transitionSpeed;
        }

        sprintWeight = Mathf.Clamp01(sprintWeight);
    }

    void Bob()
    {
        bobPosition += Time.deltaTime;
        float yWalk = Mathf.Sin(2 / walkLength * 2 * Mathf.PI * bobPosition) * amplitude;
        float xWalk = Mathf.Sin(0.5f / walkLength * (bobPosition - Mathf.PI / 2f)) * amplitude / 2f;
        float ySprint = Mathf.Sin(2 / sprintLength * 2 * Mathf.PI * bobPosition) * sAmplitude;
        float xSprint = Mathf.Sin(0.5f / sprintLength * (bobPosition - Mathf.PI / 2f)) * sAmplitude / 2f;
        float y = (ySprint - yWalk) * sprintWeight + yWalk;
        float x = (xSprint - xWalk) * sprintWeight + xWalk;
        transform.localPosition = new Vector3(x, y, 0f);
    }
    
    void ResetPos()
    {
        bobPosition = 0f;
        if (transform.localPosition != Vector3.zero)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * resetSpeed);
        }
    }
}
