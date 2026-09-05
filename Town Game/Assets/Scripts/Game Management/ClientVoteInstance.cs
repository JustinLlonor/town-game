using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System;

public struct ClientVoteInstance : INetworkStruct
{
    public int id;
    public NetworkString<_32> voteAction;
    public NetworkString<_32> iconId;
    public float voteTimeEnd;
    [Networked, Capacity(15)] public NetworkLinkedList<PlayerRef> votedWhitelist => default;

    public ClientVoteInstance(int id, NetworkString<_32> voteAction, NetworkString<_32> iconId, float voteTimeEnd, List<PlayerRef> votedWhitelist)
    {
        this.id = id;
        this.voteAction = voteAction;
        this.iconId = iconId;
        this.voteTimeEnd = voteTimeEnd;
        foreach (PlayerRef player in votedWhitelist) this.votedWhitelist.Add(player);
    }
}
