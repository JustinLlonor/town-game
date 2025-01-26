using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ServerManager : NetworkBehaviour
{
    [Networked] public string steamID { get; set; }

    public override void Spawned()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            steamID = SessionData.steamIdLobby.ToString();
        }
    }
}
