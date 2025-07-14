using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[System.Serializable]
public class CameraShot : SceneElement
{
    [Header("Camera Shot")]
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;
    //public bool animated;
    //public Vector3 endPosition;
    //public Quaternion endRotation;
    //public string animCurve;

    /**
    /// <summary>
    /// Creates an unmoving camera shot
    /// </summary>
    /// <param name="time"></param>
    /// <param name="length"></param>
    /// <param name="spawnPosition">The initial camera location</param>
    /// <param name="spawnRotation">The initial camera rotation</param>
    public CameraShot(float time, float length, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        this.time = time;
        this.length = length;
        this.spawnPosition = spawnPosition;
        this.spawnRotation = spawnRotation;
        animated = false;
        endPosition = Vector3.zero;
        endRotation = Quaternion.identity;
        animCurve = null;
    }

    /// <summary>
    /// Creates a moving camera shot
    /// </summary>
    /// <param name="time"></param>
    /// <param name="length"></param>
    /// <param name="spawnPosition">The initial camera location</param>
    /// <param name="spawnRotation">The initial camera rotation</param>
    /// <param name="endPosition">The final rotation of the camera</param>
    /// <param name="endRotation">The final rotation of the camera</param>
    /// <param name="animCurve">The name of the animation curve found in object manager for this shot to use</param>
    public CameraShot(float time, float length, Vector3 spawnPosition, Quaternion spawnRotation, Vector3 endPosition, Quaternion endRotation, string animCurve)
    {
        this.time = time;
        this.length = length;
        this.spawnPosition = spawnPosition;
        this.spawnRotation = spawnRotation;
        this.endPosition = endPosition;
        this.endRotation = endRotation;
        this.animCurve = animCurve;
        animated = true;
    }
    **/
}
