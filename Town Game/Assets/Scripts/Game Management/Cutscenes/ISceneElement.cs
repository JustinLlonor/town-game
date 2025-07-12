using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISceneElement
{
    float GetTime();
    float SetTime(float time);
    float GetLength();
    float SetLength(float length);
}
