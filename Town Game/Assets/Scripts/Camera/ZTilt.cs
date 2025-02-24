using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZTilt : MonoBehaviour
{
    public float maxDeg = 10f;
    public float camXMultiplier = 2.5f;
    public float lerpSpeed = 10f;
    float desiredZ = 0f;
    Vector2 cm = new Vector2();

    public void ReceiveCM(Vector2 cameraMovement)
    {
        cm.x = cameraMovement.x;
        cm.y = cameraMovement.y;
    }

    private void Update()
    {
        desiredZ = Mathf.Clamp(cm.x * camXMultiplier, -maxDeg, maxDeg);
        float newZ = Mathf.LerpAngle(transform.eulerAngles.z, desiredZ, Time.deltaTime * lerpSpeed);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, newZ);
    }
}
