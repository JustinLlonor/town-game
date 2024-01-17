using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPan : MonoBehaviour
{
    public float panSpeed = 1f;
    public float timeLimit = 5f;
    public float zTiltSpeed = 1f;

    private void Update()
    {
        if (timeLimit < 0f) return;
        timeLimit -= Time.deltaTime;
        transform.position = transform.position + transform.forward * panSpeed * Time.deltaTime;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + Time.deltaTime * zTiltSpeed);
    }
}
