using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ActionHolder : NetworkBehaviour
{
    public IntAction[] actions = new IntAction[0];
    [Networked, Capacity(6)] public NetworkLinkedList<NIActionInfo> actionInfo => default;
    public bool canInteract = true;
    public bool despawned = false;
    /// <summary>
    /// Called on the client when the player looks at this action holder initially
    /// </summary>
    public PositionEvent onLook;
    /// <summary>
    /// Called every frame the client looks at this action holder
    /// </summary>
    public PositionEvent onLookContinue;
    /// <summary>
    /// Called on the client when the player stops looking at this action holder
    /// </summary>
    public ActionHolderEvent onUnlook;

    public delegate void ActionHolderEvent();
    public delegate void PositionEvent(Vector3 position);

    public override void Spawned()
    {
        if (!Runner.IsServer) return;
        for (int i = 0; i < actions.Length; i++)
        {
            // not is client is a server action, add server action data
            if (!actions[i].isClient)
            {
                actions[i].actionInfoIndex = actionInfo.Count;
                IntAction action = actions[i];
                actionInfo.Add(new NIActionInfo(action.enabled, action.usePlayerLimiters, action.useTimeModify, action.length));
                continue;
            }
            actions[i].actionInfoIndex = -1;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        despawned = true;
    }

    public IntAction GetAction(string actionName)
    {
        foreach (IntAction action in actions)
        {
            if (action.actionName == actionName) return action;
        }
        return null;
    }

    [ContextMenu("Add Default Action")]
    public void AddDefaultAction()
    {
        actions = new IntAction[] { new IntAction() };
    }
}
