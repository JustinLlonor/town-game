using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TaskFinishInfo : INetworkStruct
{
    public float reward;
    public int strikes;
    public NetworkString<_64> strikeReason;
    public NetworkString<_64> rewardReason;
    [Networked, Capacity(12)] public NetworkLinkedList<Task> associatedTasks => default;

    public TaskFinishInfo(float reward, int strikes, string strikeReason = "", string rewardReason = "All objectives compeleted.")
    {
        this.reward = reward;
        this.strikes = strikes;
        this.strikeReason = strikeReason;
        this.rewardReason = rewardReason;
    }
}
