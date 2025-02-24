using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZTilt : MonoBehaviour
{
    public CameraBobbing cb;
    public float maxDeg = 10f;
    public float camXMultiplier = 2.5f;
    public float stepSpeed = 5f;
    float desiredZ = 0f;
    float cmX = 0f;
    int tFrame = 30;

    private void Start()
    {
        Application.targetFrameRate = 30;
    }

    public void ReceiveCM(Vector2 cameraMovement)
    {
        cmX = cameraMovement.x;
    }

    private void Update()
    {
        if (!cb.isCrouching) desiredZ = Mathf.Clamp((cmX * camXMultiplier) / (Time.deltaTime * 90f), -maxDeg, maxDeg); // Consistent with all mouse speeds
        else desiredZ = 0f;
        float newZ = Mathf.MoveTowardsAngle(transform.eulerAngles.z, desiredZ, Time.deltaTime * stepSpeed);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, newZ);
    }
}
