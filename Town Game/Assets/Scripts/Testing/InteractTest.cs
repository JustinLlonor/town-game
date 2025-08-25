using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class InteractTest : NetworkBehaviour
{
    private void Awake()
    {
        GetComponent<ActionHolder>().GetAction("Interact").onInteract += Interaction;
        GetComponent<ActionHolder>().GetAction("Interact2").onInteract += Interaction;
        GetComponent<ActionHolder>().GetAction("Interact3").onInteract += Interaction;
        GetComponent<ActionHolder>().GetAction("Interact4").onInteract += Interaction2;
        GetComponent<ActionHolder>().GetAction("Interact5").onInteract += Interaction2;
    }

    public void Interaction(Player player)
    {
        Debug.LogError("Sup guys");
    }

    public void Interaction2(Player player)
    {
        Debug.LogError("Sup guys the sequel");
    }
}
