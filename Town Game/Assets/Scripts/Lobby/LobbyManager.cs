using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using WebSocketSharp;
using Steamworks;
using System;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject loadingScreen;
    public GameObject lobby;
    public TextMeshProUGUI createText;
    public TextMeshProUGUI joinText;
    public TMP_InputField nicknameText;
    string previousNick;

    private void FixedUpdate()
    {
        if (SteamManager.Initialized) SteamAPI.RunCallbacks();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        PhotonNetwork.ConnectUsingSettings();
        string defaultNick = "";
        if (SteamManager.Initialized) defaultNick = SteamFriends.GetPersonaName();
        SessionData.nickname = defaultNick;
        previousNick = SessionData.nickname;
    }

    public void OnChangedNickname(string newNick)
    {
        bool nickValid = true;
        if (newNick.Length > 32) nickValid = false;
        if (newNick.IsNullOrEmpty()) nickValid = false;
        if (!nickValid)
        {
            nicknameText.text = previousNick;
            return;
        }
        SessionData.nickname = newNick.Trim();
        nicknameText.text = SessionData.nickname;
    }

    public void CreatePress()
    {
        if (nicknameText.text.IsNullOrEmpty()) return;
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 15;
        ro.CleanupCacheOnLeave = false;
        if (SteamManager.Initialized)
        {
            ro.IsOpen = false;
            ro.IsVisible = false;
        }
        PhotonNetwork.CreateRoom(createText.text, ro);
        if (SteamManager.Initialized) SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 15);
    }

    public void JoinPress()
    {
        if (nicknameText.text.IsNullOrEmpty()) return;
        Debug.Log("Joining");
        Debug.Log(PhotonNetwork.IsConnected);
        PhotonNetwork.JoinRoom(joinText.text);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to " + PhotonNetwork.CloudRegion + "!");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        loadingScreen.SetActive(false);
        lobby.SetActive(true);
        nicknameText.text = SessionData.nickname;
    }

    public override void OnCreatedRoom()
    {
        //SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 15);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel(1);
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
        // Initialize Properties
        playerProperties["name"] = SessionData.nickname;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError(message);    
    }
}
