using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using WebSocketSharp;

/// <summary>
/// Deals with progress handlers
/// </summary>
public class PlayerProgress : NetworkBehaviour
{
    public InteractableFinder inf;
    public float progressDistance = 2f;
    public LayerMask environmentLayer;
    [Networked] public bool progressing { get; set; }
    [Networked] public NetworkId targettedHandler { get; set; }
    [Networked] public bool progressPrimaryUse { get; set; }
    ProgressManager progressManager;
    ObjectManager objectManager;

    // Do 1 cast on the initial click, if it hits something, then start a progression.

    public override void Spawned()
    {
        progressManager = FindAnyObjectByType<ProgressManager>();
        objectManager = FindAnyObjectByType<ObjectManager>();
    }

    public void InitialCastCheck(bool primaryUse, string heldItem)
    {
        Transform castTransform = inf.trackedTransform;
        Vector3 castDirection = inf.forwardDirection;
        RaycastHit hit;
        if (!Physics.Raycast(castTransform.position, castDirection, out hit, progressDistance, environmentLayer)) return;
        GameObject hitObject = hit.collider.gameObject;
        ProgressHandler hitHandler = progressManager.GetProgressHandler(hitObject);
        if (hitHandler == null) return; // if it doesn't have a handler, don't start progressing
        if (!hitHandler.canProgress) return; // if the handler can't progress, then don't start
        Item heldItemObject = null;
        if (!heldItem.IsNullOrEmpty())
        {
            heldItemObject = objectManager.itemSearch[heldItem];
        }
        if (!hitHandler.UseChanges(heldItemObject, primaryUse)) return; // if the use case can't change anything, return
        targettedHandler = progressManager.GetHandlerId(hitObject);
        progressPrimaryUse = primaryUse;
        StartProgress();
    }

    public void ContinuationCheck(bool primaryUse, string heldItem)
    {
        Transform castTransform = inf.trackedTransform;
        Vector3 castDirection = inf.forwardDirection;
        RaycastHit hit;
        if (!Physics.Raycast(castTransform.position, castDirection, out hit, progressDistance, environmentLayer))
        {
            StopProgress();
            return;
        }
        GameObject hitObject = hit.collider.gameObject;
        ProgressHandler hitHandler = progressManager.GetProgressHandler(hitObject);
        // Stop progress if player looks at another object without handler
        if (hitHandler == null)
        {
            StopProgress();
            return;
        }
        // If the hit object id on this continuation check is not equal to the current targetted one, then stop
        if (!(progressManager.GetHandlerId(hitObject).Equals(targettedHandler)))
        {
            StopProgress();
            return;
        }
        if (progressPrimaryUse != primaryUse)
        {
            StopProgress();
            return;
        }
        // It is assumed after this point that the player is still looking at the progress object
        Item heldItemObject = null;
        if (!heldItem.IsNullOrEmpty())
        {
            heldItemObject = objectManager.itemSearch[heldItem];
        }
        bool canProgress = hitHandler.ProcessProgress(heldItemObject, primaryUse, Runner.DeltaTime);
        if (!canProgress)
        {
            StopProgress();
            return;
        }
    }

    public void StartProgress()
    {
        progressing = true;
    }

    /// <summary>
    /// Stops the progress of this player
    /// </summary>
    public void StopProgress()
    {
        progressing = false;
    }
}
