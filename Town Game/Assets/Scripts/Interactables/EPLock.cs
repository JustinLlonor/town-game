using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines the lock of an entry point
[RequireComponent(typeof(EntryPoint))]
public class EPLock : NetworkBehaviour
{
    public int[] allowedKeys = new int[0];
    Grabbable grabbable;
    ProgressHandler progress;

    public override void Spawned()
    {
        grabbable = GetComponent<Grabbable>();
        progress = GetComponent<ProgressHandler>();
        grabbable.playerGrabChecks += KeyConditional;
        progress.playerSkipChecks += KeyConditional;
    }

    /// <summary>
    /// The conditional function that determines if the player can enter this entry point
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private bool KeyConditional(Player player)
    {
        PlayerRef opener = player.owner;
        foreach (int key in allowedKeys)
        {
            if (PlayerManager.i.PlayerHasKey(opener, key)) return true; // can grab, and can skip progress
        }
        return false;
    }
    
}
