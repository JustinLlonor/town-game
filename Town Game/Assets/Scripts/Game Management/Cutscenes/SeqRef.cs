using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct SeqRef : INetworkStruct
{
    public RefType refType;
    public int id;

    public SeqRef(PlayerRef player)
    {
        refType = RefType.Player;
        id = player.AsIndex;
    }
}
