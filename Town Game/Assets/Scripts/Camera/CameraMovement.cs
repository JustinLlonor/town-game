using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Voice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float mouseSensitivity = 1f;
    public Transform player;
    public Transform orientation;
    public Transform headAim;

    float xRotation = 0f;
    float yRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (player == null) return;
        CameraLook();
    }

    void CameraLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        yRotation += mouseX;
        transform.eulerAngles = new Vector3(xRotation, yRotation, 0);
        orientation.eulerAngles = new Vector3(0, yRotation, 0);
        player.eulerAngles = orientation.eulerAngles;
        headAim.position = transform.position + transform.forward;
    }
}
