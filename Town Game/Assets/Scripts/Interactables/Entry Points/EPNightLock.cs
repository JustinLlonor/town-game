using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines the lock of an entry point
[RequireComponent(typeof(EntryPoint))]
public class EPNightLock : NetworkBehaviour
{
    public int[] allowedKeys = new int[0];
    Grabbable grabbable;
    ProgressHandler progress;

    public override void Spawned()
    {
        grabbable = GetComponent<Grabbable>();
        progress = GetComponent<ProgressHandler>();
        grabbable.playerGrabChecks += NightConditional;
        progress.playerSkipChecks += NightConditional;
    }

    /// <summary>
    /// The conditional function that determines if the player can enter this entry point
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private bool NightConditional(Player player)
    {
        return GameManager.i.isDay; // if day, then grab and progress skip can happen
    }

}
