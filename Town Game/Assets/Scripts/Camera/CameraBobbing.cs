using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    public float frequency = 1f;
    public float sFrequency = 1f;
    public float amplitude = .1f;
    public float sAmplitude = .1f;
    public float snapSpeed = 20f;
    public float snapFix = 30f;
    public bool isBobbing = false;
    public bool isSprinting = false;
    float bobPosition;
    float fixSpeed = 0f;

    private void Update()
    {
        if (isBobbing) Bob();
        if (!isBobbing) ResetPos();
    }

    void Bob()
    {
        float usedFrequency;
        float usedAmplitude;
        if (isSprinting)
        {
            usedFrequency = sFrequency;
            usedAmplitude = sAmplitude;
        }
        else
        {
            usedFrequency = frequency;
            usedAmplitude = amplitude;
        }
        bobPosition += Time.deltaTime;
        float y = Mathf.Sin(bobPosition/usedFrequency) * usedAmplitude;
        float x = Mathf.Sin(0.5f/usedFrequency * (bobPosition - Mathf.PI/2f)) * usedAmplitude/2f;
        Vector3 desiredPos = new Vector3(x, y, 0f);
        if (Vector3.Distance(desiredPos, transform.localPosition) > 0.01f)
        {
            fixSpeed += snapFix * Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPos, Time.deltaTime * fixSpeed);
        } 
        else
        {
            transform.localPosition = desiredPos;
            fixSpeed = 0f;
        }
    }

    void ResetPos()
    {
        bobPosition = 0f;
        if (transform.localPosition != Vector3.zero)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * snapSpeed);
        }
    }
}
