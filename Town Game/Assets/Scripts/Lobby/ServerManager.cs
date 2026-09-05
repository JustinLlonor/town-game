using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Steamworks;
using System;

public class ServerManager : NetworkBehaviour
{
    [Networked] public string steamID { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            steamID = SessionData.steamIdLobby.ToString();
        }
        if (!SteamManager.Initialized) return;
        ulong usedSteamID = Convert.ToUInt64(steamID);
        Debug.Log("Joining Steam lobby: " + steamID);
        SteamMatchmaking.JoinLobby((CSteamID)usedSteamID);
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            steamID = SessionData.steamIdLobby.ToString();
        }
    }
}
