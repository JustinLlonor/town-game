using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct SeqScene : INetworkStruct
{
    public Vector3 camPosition;
    public Quaternion camRotation;
    public int sceneIndex;
    [Capacity(8)] NetworkDictionary<int, SeqRef> references => default;
}