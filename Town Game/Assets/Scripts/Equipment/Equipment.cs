using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Equipment : NetworkBehaviour
{
    [Networked] public float HP { get; set; }
    public MapRoom room;
}
