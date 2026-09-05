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
    public NetworkBool usePlayerLimiters;
    public NetworkBool useTimeModify;
    public float defaultTime;

    public NIActionInfo(bool enabled, bool usePlayerLimiters, bool useTimeModify, float defaultTime)
    {
        this.enabled = enabled;
        this.usePlayerLimiters = usePlayerLimiters;
        this.useTimeModify = useTimeModify;
        this.defaultTime = defaultTime;
    }

    [Networked, Capacity(15)] public NetworkLinkedList<PlayerRef> playerLimiters => default;
    [Networked, Capacity(15)] public NetworkDictionary<PlayerRef, float> interactLengths => default;

    /// <summary>
    /// Gets if the player can interact with this interactable
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool CanInteract(PlayerRef player)
    {
        if (!enabled) return false;
        if (usePlayerLimiters) // if player limiters = true and the player is not within the limiter, can't interact
        {
            if (!playerLimiters.Contains(player)) return false;
        }
        return true;
    }

    /// <summary>
    /// Gets the interaction length of this action for a certain player
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public float GetInteractLength(PlayerRef player)
    {
        if (!useTimeModify)
        {
            return defaultTime;
        }
        if (!interactLengths.ContainsKey(player))
        {
            return defaultTime;
        }
        return interactLengths[player];
    }
}
