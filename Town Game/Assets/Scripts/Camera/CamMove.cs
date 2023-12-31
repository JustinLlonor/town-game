using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMove : MonoBehaviour
{
    public Transform camPos;

    void Update()
    {
        if (camPos != null) transform.position = camPos.position;
    }
}
