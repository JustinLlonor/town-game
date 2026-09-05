using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GrabPoint : NetworkBehaviour
{
    [Networked] public bool isOpen { get; set; } = false;
    public Grabbable grabbable;
}
