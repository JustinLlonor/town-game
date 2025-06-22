using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZTilt : MonoBehaviour
{
    public CameraBobbing cb;
    public CameraManager cm;
    public float maxDeg = 10f;
    public float camXMultiplier = 2.5f;
    public float stepSpeed = 5f;
    public bool canTurn = true;
    float desiredZ = 0f;
    float cmX = 0f;

    private void Awake()
    {
        UIManager um = FindFirstObjectByType<UIManager>();
        um.OnUIOpen += SetCanTurnFalse;
        um.OnUIClose += SetCanTurnTrue;
    }

    void SetCanTurnTrue()
    {
        canTurn = true;
    }

    void SetCanTurnFalse(int i)
    {
        canTurn = false;
    }

    public void ReceiveCM(Vector2 cameraMovement)
    {
        cmX = cameraMovement.x;
    }

    private void Update()
    {
        desiredZ = Mathf.Clamp((cmX * camXMultiplier) / (Time.deltaTime * 90f), -maxDeg, maxDeg); // Consistent with all mouse speeds
        if (!canTurn) desiredZ = 0f;
        if (cm.mode != CameraManager.CameraMode.FirstPerson) desiredZ = 0f;
        //if (!cb.isCrouching) desiredZ = Mathf.Clamp((cmX * camXMultiplier) / (Time.deltaTime * 90f), -maxDeg, maxDeg); // Consistent with all mouse speeds
        //else desiredZ = 0f;
        float newZ = Mathf.MoveTowardsAngle(transform.localRotation.eulerAngles.z, desiredZ, Time.deltaTime * stepSpeed);
        transform.localRotation = Quaternion.Euler(new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, newZ));
    }
}
