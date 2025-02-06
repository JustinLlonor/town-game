using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
using Steamworks;
using System;
using Fusion;
using WebSocketSharp;

public class SteamLobbyManager : MonoBehaviour
{
    public NetworkRunner runner;
    ServerManager sm;
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<LobbyEnter_t> LobbyEntered;
    protected Callback<GameLobbyJoinRequested_t> LobbyRequested;

    private new void OnEnable()
    {
        if (SteamManager.Initialized)
        {
            Debug.Log("Creating callbacks...");
            LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            LobbyRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyRequested);
        }
    }

    private new void OnDisable()
    {
        if (SteamManager.Initialized)
        {
            Debug.Log("Disposing callbacks...");
            LobbyCreated.Dispose();
            LobbyEntered.Dispose();
            LobbyRequested.Dispose();
        }

    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        SteamMatchmaking.SetLobbyData((CSteamID)callback.m_ulSteamIDLobby, "roomname", runner.SessionInfo.Name);
        // Sets the steam lobby id in photon lobby
        sm.steamID = callback.m_ulSteamIDLobby.ToString();
        SessionData.steamIdLobby = Convert.ToUInt64(sm.steamID);
        //ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        //roomProperties["steamID"] = callback.m_ulSteamIDLobby.ToString();
        //PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        // Makes the room joinable
        runner.SessionInfo.IsOpen = true;
        runner.SessionInfo.IsVisible = true;
        //PhotonNetwork.CurrentRoom.IsVisible = true;
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        SessionData.steamIdLobby = callback.m_ulSteamIDLobby;
        Debug.LogError("Joined steam lobby with ID: " + callback.m_ulSteamIDLobby);
        Debug.Log((string)SteamMatchmaking.GetLobbyData((CSteamID)callback.m_ulSteamIDLobby, "roomname") + " Data");
    }

    private void OnLobbyRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.LogError("Hi (with rizz)");
    }

    private void Start()
    {
        sm = FindFirstObjectByType<ServerManager>();
        runner = FindFirstObjectByType<NetworkRunner>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            InviteDialog();
        }
    }

    /// <summary>
    /// Steam invite menu
    /// </summary>
    public void InviteDialog()
    {
        if (SteamManager.Initialized) SteamFriends.ActivateGameOverlayInviteDialog((CSteamID)SessionData.steamIdLobby);
        
    }

    //public override void OnLeftLobby()
    //{
    //    SteamMatchmaking.LeaveLobby((CSteamID)SessionData.steamIdLobby);
    //}
}
