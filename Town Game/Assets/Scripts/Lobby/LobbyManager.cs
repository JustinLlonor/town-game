using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//using Photon.Pun;
//using Photon.Realtime;
using WebSocketSharp;
using Steamworks;
using Fusion;

public class LobbyManager : MonoBehaviour//PunCallbacks
{
    public GameObject loadingScreen;
    public GameObject lobby;
    public TextMeshProUGUI createText;
    public TextMeshProUGUI joinText;
    private RunnerManager runnerManager;

    private void FixedUpdate()
    {
        if (SteamManager.Initialized) SteamAPI.RunCallbacks();
    }

    private void Start()
    {
        string defaultNick = "";
        if (SteamManager.Initialized) defaultNick = SteamFriends.GetPersonaName();
        SessionData.nickname = defaultNick;
        runnerManager = FindFirstObjectByType<RunnerManager>();
    }

    public void CreatePress()
    {
        if (SteamManager.Initialized) SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 15);
        runnerManager.StartGame(GameMode.Host, createText.text, runnerManager.waitingRoomIndex);
    }

    public void JoinPress()
    {
        Debug.Log("Joining");
        runnerManager.StartGame(GameMode.Client, joinText.text, runnerManager.waitingRoomIndex);
    }

    public void TestPress()
    {
        runnerManager.StartGame(GameMode.Single, joinText.text, runnerManager.testRoomIndex);
    }
}
