using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using WebSocketSharp;
using Steamworks;
using System.Text;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject loadingScreen;
    public GameObject lobby;
    public TextMeshProUGUI createText;
    public TextMeshProUGUI joinText;
    public TMP_InputField nicknameText;
    string previousNick;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        CSteamID steamID = SteamUser.GetSteamID();
        string defaultNick = SteamFriends.GetPersonaName();
        string newNick = SteamFriends.GetPlayerNickname(steamID);
        SessionData.nickname = defaultNick;
        if (newNick != null) SessionData.nickname = newNick;
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
        ro.CleanupCacheOnLeave = false;
        PhotonNetwork.CreateRoom(createText.text, ro);
    }

    public void JoinPress()
    {
        if (nicknameText.text.IsNullOrEmpty()) return;
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
}
