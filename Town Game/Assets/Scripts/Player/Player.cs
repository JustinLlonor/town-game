using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public delegate void PlayerEvent();
    public PlayerEvent Init;

    public override void Spawned()
    {
        Init?.Invoke();
    }
}
