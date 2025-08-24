using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ActionHolder : NetworkBehaviour
{
    public IntAction[] actions = new IntAction[0];
    [Networked, Capacity(6)] public NetworkLinkedList<NIActionInfo> actionInfo => default;
    public bool canInteract = true;


    public override void Spawned()
    {
        if (!Runner.IsServer) return;
        for (int i = 0; i < actions.Length; i++)
        {
            // not is client is a server action, add server action data
            if (!actions[i].isClient)
            {
                actions[i].actionIndex = i;
                actionInfo.Add(new NIActionInfo(actions[i].enabled));
                continue;
            }
            actions[i].actionIndex = -1;
        }
    }

    [ContextMenu("Add Default Action")]
    public void AddDefaultAction()
    {
        actions = new IntAction[] { new IntAction() };
    }
}
