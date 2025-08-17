using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Grabbable : NetworkBehaviour
{
    [Networked] public bool canGrab { get; set; } = true;
    [Networked] public Vector3 grabPoint { get; set; }
    [Networked] public PlayerRef grabber { get; set; } = PlayerRef.None;

    public void EnableGrab()
    {
        canGrab = true;
    }

    public void DisableGrab()
    {
        canGrab = false;
    }
}
