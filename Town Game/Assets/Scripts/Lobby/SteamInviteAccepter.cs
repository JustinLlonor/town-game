using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Photon.Pun;
using WebSocketSharp;

public class SteamInviteAccepter : MonoBehaviour
{
    protected Callback<LobbyEnter_t> LobbyEntered;
    protected Callback<GameLobbyJoinRequested_t> AcceptedInvite;

    private void OnEnable()
    {
        if (SteamManager.Initialized)
        {
            AcceptedInvite = Callback<GameLobbyJoinRequested_t>.Create(OnAcceptedInvite);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }
    }

    private void OnDisable()
    {
        if (SteamManager.Initialized)
        {
            AcceptedInvite.Dispose();
            LobbyEntered.Dispose();
        }
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        string roomName = (string)SteamMatchmaking.GetLobbyData((CSteamID)callback.m_ulSteamIDLobby, "roomname");
        if (roomName.IsNullOrEmpty()) return;
        Debug.LogError("Accepting invite to room: " + roomName);
        PhotonNetwork.JoinRoom(roomName);
    }

    private void OnAcceptedInvite(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
}
