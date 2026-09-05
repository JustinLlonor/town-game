using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneElement
{
    public float time;
    public float length;

    /// <summary>
    /// Returns a number from 0 to 1 of how much of this element has passed
    /// </summary>
    /// <param name="localTime"></param>
    /// <returns></returns>
    public float GetProgress(float localTime)
    {
        return Mathf.Clamp01((localTime - time) / length);
    }
}
