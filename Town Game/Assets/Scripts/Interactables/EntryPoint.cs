using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

/// <summary>
/// Defines a grabbable entry point that uses the progress handler system.
/// </summary>
[RequireComponent(typeof(Grabbable))]
[RequireComponent(typeof(ProgressHandler))]
public class EntryPoint : NetworkBehaviour
{
    [Networked] public bool isOpen { get; set; } = false;
    Grabbable grabbable;
    ProgressHandler progressHandler;

    public override void Spawned()
    {
        grabbable = GetComponent<Grabbable>();
        progressHandler = GetComponent<ProgressHandler>();
        grabbable.EnableGrab();
        progressHandler.onThresholdReach += ProgressThresholdReach;
        progressHandler.onThresholdPassSubtract += ProgressThresholdSubtract;
        if (!progressHandler.eventThresholds.Contains(100f)) Debug.LogError("Progress handler does not contain a threshold event for completion!");
    }

    private void ProgressThresholdReach(float threshold)
    {
        
    }

    private void ProgressThresholdSubtract(float threshold)
    {
        
    }
}
