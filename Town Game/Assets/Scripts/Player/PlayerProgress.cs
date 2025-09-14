using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using WebSocketSharp;
using UnityEngine.InputSystem;

/// <summary>
/// Deals with progress handlers
/// </summary>
public class PlayerProgress : NetworkBehaviour
{
    public Player player;
    public InteractableFinder inf;
    public PlayerInventory inventory;
    public InputActionReference primaryProgress;
    public InputActionReference secondaryProgress;
    public float progressDistance = 2f;
    public LayerMask environmentLayer;
    [Networked] public bool progressing { get; set; }
    [Networked] public NetworkId targettedHandler { get; set; }
    [Networked] public bool progressPrimaryUse { get; set; }
    ProgressManager progressManager;
    ObjectManager objectManager;
    public Rigidbody rb;
    private InteractableUI iui;

    public override void Spawned()
    {
        progressManager = FindAnyObjectByType<ProgressManager>();
        objectManager = FindAnyObjectByType<ObjectManager>();
        if (!HasInputAuthority) return;
        iui = FindFirstObjectByType<InteractableUI>();
    }

    public override void FixedUpdateNetwork()
    {
        ProgressVisualCheck();
    }

    public string ToInteractKey(InputActionReference inputAction)
    {
        InputAction actionRef = inputAction.action;
        int bindingIndex = actionRef.GetBindingIndexForControl(actionRef.controls[0]);
        string interactText = InputControlPath.ToHumanReadableString(
                    actionRef.bindings[bindingIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
        return interactText;
    }

    private void ProgressVisualCheck()
    {
        if (!HasInputAuthority) return;
        if (Runner.IsResimulation) return;
        ProgressModifierInfo primaryInfo;
        ProgressModifierInfo secondaryInfo;
        bool looking = ProgressVisualCast(out primaryInfo, out secondaryInfo);
        iui.DisplayProgressInteraction(primaryInfo, secondaryInfo, looking, this);
    }

    private bool ProgressVisualCast(out ProgressModifierInfo primaryInfo, out ProgressModifierInfo secondaryInfo)
    {
        Transform castTransform = inf.trackedTransform;
        Vector3 castDirection = inf.forwardDirection;
        RaycastHit hit;
        Vector3 castPosition = new Vector3(rb.position.x, castTransform.position.y, rb.position.z);
        // Set primary and secondary info
        primaryInfo = ProgressModifierInfo.None; 
        secondaryInfo = ProgressModifierInfo.None;
        if (!Physics.Raycast(castPosition, castDirection, out hit, progressDistance, environmentLayer)) return false;
        GameObject hitObject = hit.collider.gameObject;
        ProgressHandler hitHandler = progressManager.GetProgressHandler(hitObject);
        if (hitHandler == null) return false; // if it doesn't have a handler, don't show visuals
        if (!hitHandler.canProgress) return false; // if the handler can't progress, then don't show visuals
        if (hitHandler.ProgressCanSkip(player)) return false; // if the progress can be skipped by the player, return.
        Item heldItemObject = inventory.GetHeldItem();
        primaryInfo = hitHandler.GetModifierInfo(heldItemObject, true);
        secondaryInfo = hitHandler.GetModifierInfo(heldItemObject, false);
        return true;
    }

    public void InitialCastCheck(bool primaryUse, string heldItem)
    {
        Transform castTransform = inf.trackedTransform;
        Vector3 castDirection = inf.forwardDirection;
        RaycastHit hit;
        Vector3 castPosition = new Vector3(rb.position.x, castTransform.position.y, rb.position.z);
        if (!Physics.Raycast(castPosition, castDirection, out hit, progressDistance, environmentLayer)) return;
        GameObject hitObject = hit.collider.gameObject;
        ProgressHandler hitHandler = progressManager.GetProgressHandler(hitObject);
        if (hitHandler == null) return; // if it doesn't have a handler, don't start progressing
        if (!hitHandler.canProgress) return; // if the handler can't progress, then don't start
        if (hitHandler.ProgressCanSkip(player)) return; // if the progress can be skipped by the player, return.
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
        Vector3 castPosition = new Vector3(rb.position.x, castTransform.position.y, rb.position.z);
        if (!Physics.Raycast(castPosition, castDirection, out hit, progressDistance, environmentLayer))
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
