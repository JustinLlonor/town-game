using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConnectionInfo
{
    public string name;
    [Tooltip("The Y axis is the level of effect, the x axis is the % that the sender is at.")]
    public AnimationCurve effectLevels;
}
