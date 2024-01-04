using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsManager : MonoBehaviour
{
    public Transform clientItem;
    public Transform armsItemHolder;

    private void LateUpdate()
    {
        transform.eulerAngles = clientItem.eulerAngles - (clientItem.localEulerAngles - armsItemHolder.localEulerAngles);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 180f, transform.eulerAngles.z);
        Vector3 positionDifference = clientItem.position - armsItemHolder.position;
        transform.position += positionDifference;
    }
}
