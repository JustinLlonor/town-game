using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundFlash : MonoBehaviour
{
    public float flashOffset = 0.2f;
    public float flashSpeed = 1f;
    public Camera cam;
    Color backgroundColor;

    private void Start()
    {
        backgroundColor = cam.backgroundColor;
    }

    private void Update()
    {
        Color c = backgroundColor;
        c.r = Mathf.Sin(Time.time * flashSpeed) * flashOffset + c.r;
        c.g = Mathf.Sin(Time.time * flashSpeed) * flashOffset + c.g;
        c.b = Mathf.Sin(Time.time * flashSpeed) * flashOffset + c.b;
        cam.backgroundColor = c;
    }
}
