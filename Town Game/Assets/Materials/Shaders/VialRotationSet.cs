using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VialRotationSet : MonoBehaviour
{
    public Material vialMat;

    private void Update()
    {
        Vector3 point = transform.up;
        float x = Mathf.Sqrt(Mathf.Pow(point.x, 2f) + Mathf.Pow(point.z, 2f));
        float angle = Mathf.Atan2(point.y, x) - Mathf.PI/2f;
        float angleOffset = Mathf.Atan2(point.z, point.x) + Mathf.PI/2f ;
        vialMat.SetFloat("_Angle", angle);
        vialMat.SetFloat("_AngleOffset", angleOffset);
    }
}
