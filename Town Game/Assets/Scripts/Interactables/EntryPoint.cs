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
    public GrabPoint grabPoint;
    Grabbable grabbable;
    ProgressHandler progressHandler;

    public override void Spawned()
    {
        grabbable = GetComponent<Grabbable>();
        progressHandler = GetComponent<ProgressHandler>();
        grabbable.playerCheck += GrabCheck;
    }

    public override void FixedUpdateNetwork()
    {
        CheckGrabPoint();
    }

    private void CheckGrabPoint()
    {
        if (grabPoint.isOpen == isOpen) return;
        isOpen = grabPoint.isOpen;
        progressHandler.canProgress = !isOpen;
    }

    private bool GrabCheck(Player player)
    {
        if (progressHandler.progress == 100f) return true;
        return false;
    }
}
