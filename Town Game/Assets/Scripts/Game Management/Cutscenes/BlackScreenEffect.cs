using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[System.Serializable]
public class BlackScreenEffect : SceneElement
{
    [Header("Black Screen")]
    [Tooltip("The animation curve for this effect. 0 means transparent, 1 means black.")]
    public AnimationCurve fadeCurve;
}
