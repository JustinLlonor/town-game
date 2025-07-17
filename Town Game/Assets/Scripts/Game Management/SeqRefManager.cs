using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeqRefManager : NetworkBehaviour
{
    PlayerManager playerManager;

    public override void Spawned()
    {
        playerManager = FindAnyObjectByType<PlayerManager>();
    }

    public string GetRefString(SeqRef reference)
    {
        switch (reference.refType)
        {
            case RefType.Player:
                return playerManager.GetPlayerRefName(reference.id);
            default:
                return string.Empty;
        }
    }
}
