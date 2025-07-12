using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct SeqScene : INetworkStruct
{
    /// <summary>
    /// The elements/features of a scene. Maximum of 6 elements (unless you wanna increase it)
    /// </summary>
    [Networked, Capacity(5)] public NetworkLinkedList<SceneElement> elements => default;
    /// <summary>
    /// The priority of this scene. Lower numbers will be displayed first over higher numbers
    /// </summary>
    public int scenePriority;
    /// <summary>
    /// The length of this scene, in seconds
    /// </summary>
    public float length;
}