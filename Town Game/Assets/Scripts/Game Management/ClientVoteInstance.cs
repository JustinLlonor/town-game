using Fusion;
using UnityEngine;
using System.Collections.Generic;

public struct ClientVoteInstance : INetworkStruct
{
    public int id;
    public NetworkString<_128> voteAction;
    public int iconId;
    public float voteTimeEnd;
    [Networked, Capacity(20)] public NetworkLinkedList<PlayerRef> votedWhitelist => default;

    public ClientVoteInstance(int id, NetworkString<_128> voteAction, int iconId, float voteTimeEnd, List<PlayerRef> votedWhitelist)
    {
        this.id = id;
        this.voteAction = voteAction;
        this.iconId = iconId;
        this.voteTimeEnd = voteTimeEnd;
        foreach (PlayerRef player in votedWhitelist) this.votedWhitelist.Add(player);
    }
}
