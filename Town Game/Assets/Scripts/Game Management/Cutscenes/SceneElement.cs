using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct SceneElement : INetworkStruct
{
    /// <summary>
    /// The time, in seconds, this scene element activates after the scene starts
    /// </summary>
    public float time;
    /// <summary>
    /// The length of this element in seconds
    /// </summary>
    public float length;
    /// <summary>
    /// Values, encoded through the interface where order matters
    /// </summary>
    [Networked, Capacity(8)] public NetworkArray<NetworkString<_128>> values => default;
}
