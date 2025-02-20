using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemObservable : Observable
{
    public GameObject[] siObjects;
    [Networked, Capacity(32)] public NetworkLinkedList<float> siProgress => default;
    [Networked] public int currentSI { get; set; }

    /// <summary>
    /// For when the cursor is hovering over a subinteractable
    /// </summary>
    /// <param name="si"></param>
    public void ReceiveInteractable(GameObject si)
    {
        
    }
}
