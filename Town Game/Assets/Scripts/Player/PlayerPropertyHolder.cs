using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerPropertyHolder : NetworkBehaviour
{
    [Networked] public NetworkString<_32> nickname { get; set; }
    [Networked] public bool isCultist { get; set; }
    [Networked] public int room { get; set; }
    [Networked] public int money { get; set; }
    [Networked, Capacity(20)] public NetworkLinkedList<int> groups => default;
    [Networked] public int energy { get; set; }
}
