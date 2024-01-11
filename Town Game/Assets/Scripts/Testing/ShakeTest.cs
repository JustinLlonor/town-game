using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeTest : MonoBehaviour
{
    public Shake shake;
    public CameraShake camShake;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            camShake.StartShake(shake.shakeProperties);
        }
    }
}
