using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Fusion;
//using Photon.Pun;
//using WebSocketSharp;

public class SteamInviteAccepter : MonoBehaviour
{
    RunnerManager rm;
    protected Callback<LobbyEnter_t> LobbyEntered;
    protected Callback<GameLobbyJoinRequested_t> AcceptedInvite;

    private void Awake()
    {
        rm = FindFirstObjectByType<RunnerManager>();
    }

    private void OnEnable()
    {
        if (SteamManager.Initialized)
        {
            AcceptedInvite = Callback<GameLobbyJoinRequested_t>.Create(OnAcceptedInvite);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }
    }

    public void DisableInvites()
    {
        if (SteamManager.Initialized)
        {
            AcceptedInvite.Dispose();
            LobbyEntered.Dispose();
        }
    }

    private void OnDisable()
    {
        DisableInvites();
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        string roomName = (string)SteamMatchmaking.GetLobbyData((CSteamID)callback.m_ulSteamIDLobby, "roomname");
        Debug.LogError("Accepting invite to room: " + roomName);
        rm.StartGame(GameMode.Client, roomName, 1);
    }

    private void OnAcceptedInvite(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
}
