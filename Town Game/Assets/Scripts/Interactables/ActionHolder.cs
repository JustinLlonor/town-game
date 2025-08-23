using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ActionHolder : NetworkBehaviour
{
    public NIAction[] actions = new NIAction[0];
    [Networked, Capacity(6)] public NetworkLinkedList<NIActionInfo> actionInfo => default;

    public override void Spawned()
    {
        if (!Runner.IsServer) return;
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].actionIndex = i;
            actionInfo.Add(new NIActionInfo(actions[i].defaultEnabled));
        }
    }

    [ContextMenu("Add Default Action")]
    public void AddDefaultAction()
    {
        actions = new NIAction[] { new NIAction() };
    }
}
