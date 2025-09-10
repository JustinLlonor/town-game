using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class InteractTest : NetworkBehaviour
{
    private void Awake()
    {
        GetComponent<ActionHolder>().GetAction("Interact").onInteract += Interaction;
        GetComponent<ActionHolder>().GetAction("Axe!").onInteract += Interaction2;
    }

    public void Interaction(Player player)
    {
        Debug.LogError("Sup guys");
    }

    public void Interaction2(Player player)
    {
        Debug.LogError("You used the axe!");
    }
}
