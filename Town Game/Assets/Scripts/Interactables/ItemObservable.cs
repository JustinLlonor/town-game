using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemObservable : Observable
{
    public GameObject[] subInteractables;
    GameObject currentSI;

    /// <summary>
    /// For when the cursor is hovering over a subinteractable
    /// </summary>
    /// <param name="si"></param>
    public void ReceiveInteractable(GameObject si)
    {
        
    }
}
