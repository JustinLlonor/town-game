using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Network Interactable Action
/// </summary>
public struct NIActionInfo : INetworkStruct
{
    public NetworkBool enabled;

    public NIActionInfo(bool enabled)
    {
        this.enabled = enabled;
    }

    [Networked, Capacity(15)] public NetworkLinkedList<PlayerRef> playerLimiters => default;
    [Networked, Capacity(15)] public NetworkDictionary<PlayerRef, float> interactLengths => default;

   
}
