using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZoomEffect : SceneElement {
    [Header("Zoom Effect")]
    [Tooltip("The distance the camera starts at. A negative value means the camera goes backwards, " +
        "and a positive value means the camera goes forwards")]
    public float startDistance = 0f;
    public float endDistance = 0f;
    public AnimationCurve zoomCurve;
}
