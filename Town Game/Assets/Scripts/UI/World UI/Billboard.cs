using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Transform targetTransform;

    private void OnEnable()
    {
        targetTransform = Camera.main.transform;
    }

    private void Update()
    {
        Quaternion rot = Quaternion.LookRotation(transform.position - targetTransform.position, Vector3.up);
        rot.eulerAngles = new Vector3(0f, rot.eulerAngles.y, rot.eulerAngles.z);
        transform.rotation = rot;
    }
}
