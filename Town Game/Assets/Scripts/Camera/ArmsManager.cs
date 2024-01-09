using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsManager : MonoBehaviour
{
    public Transform clientItem;
    public Transform armsItemHolder;
    public bool traceItem = true;

    Transform camTransform;

    private void Awake()
    {
        camTransform = Camera.main.transform;
    }

    public void FollowCam(float offset)
    {
        if (traceItem) return;
        transform.position = camTransform.position;
        transform.position -= transform.up * offset;
    }

    public void GrabItem()
    {
        if (!traceItem) return;
        Quaternion rotDifference = clientItem.rotation * Quaternion.Inverse(armsItemHolder.rotation);
        transform.rotation = rotDifference * transform.rotation;
        Vector3 positionDifference = clientItem.position - armsItemHolder.position;
        transform.position += positionDifference;
    }
}
