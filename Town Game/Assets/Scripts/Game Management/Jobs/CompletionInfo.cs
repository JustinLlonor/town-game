using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[System.Serializable]
public struct CompletionInfo : INetworkStruct
{
    public NetworkString<_8> id;
    public int performanceChange;
    public float moneyChange;
    public float punishmentPercentage; // The percentage of money subtracted for the punishment
    public NetworkBool cancelled; // if the task was cancelled

    public CompletionInfo(string id, int performanceChange, float moneyChange, float punishmentPercentage, bool cancelled)
    {
        this.id = id;
        this.performanceChange = performanceChange;
        this.moneyChange = moneyChange;
        this.punishmentPercentage = punishmentPercentage;
        this.cancelled = cancelled;
    }
}