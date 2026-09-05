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
    public EntryPointEvent onOpen;
    public EntryPointEvent onClose;
    Grabbable grabbable;
    ProgressHandler progressHandler;

    public delegate void EntryPointEvent();

    public override void Spawned()
    {
        grabbable = GetComponent<Grabbable>();
        progressHandler = GetComponent<ProgressHandler>();
        grabbable.playerGrabChecks += GrabCheck;
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.IsResimulation) return;
        CheckGrabPoint();
    }

    private void CheckGrabPoint()
    {
        if (grabPoint.isOpen == isOpen) return;
        isOpen = grabPoint.isOpen;
        progressHandler.canProgress = !isOpen;
        if (isOpen) onOpen?.Invoke();
        else onClose?.Invoke();
    }

    private bool GrabCheck(Player player)
    {
        if (progressHandler.progress == 100f) return true;
        return false;
    }
}
