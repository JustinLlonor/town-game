using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    public CameraMovement cm;
    public float swayMultiplier;
    public float smooth;

    //TODO: make WeaponSway work with new input system
    private void Update()
    {
        if (cm.lockX) return;
        float mouseX = Input.GetAxisRaw("Mouse X") * cm.mouseSensitivity * swayMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * cm.mouseSensitivity * swayMultiplier;
        if (cm.lockedPlayer != null)
        {
            mouseX = 0f;
            mouseY = 0f;
        }

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
    }
}
