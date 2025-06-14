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
    public bool testSingle = true;
    private RunnerManager runnerManager;

    private void FixedUpdate()
    {
        if (SteamManager.Initialized) SteamAPI.RunCallbacks();
    }

    private void Start()
    {
        List<int> tsetARray = new List<int>() { 43 };
        ModifyIntArray(tsetARray);
        Debug.Log(tsetARray[0]);
        string defaultNick = "";
        if (SteamManager.Initialized) defaultNick = SteamFriends.GetPersonaName();
        SessionData.nickname = defaultNick;
        runnerManager = FindFirstObjectByType<RunnerManager>();
    }

    private void ModifyIntArray(List<int> array)
    {
        array[0] = 69;
    }

    public void CreatePress()
    {
        SessionData.isTesting = false;
        if (SteamManager.Initialized) SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 15);
        runnerManager.StartGame(GameMode.Host, createText.text, runnerManager.waitingRoomIndex);
    }

    public void JoinPress()
    {
        SessionData.isTesting = false;
        Debug.Log("Joining");
        runnerManager.StartGame(GameMode.Client, joinText.text, runnerManager.waitingRoomIndex);
    }

    public void TestPress()
    {
        SessionData.isTesting = true;
        if (testSingle) runnerManager.StartGame(GameMode.Single, joinText.text, runnerManager.testRoomIndex);
        if (!testSingle) runnerManager.StartGame(GameMode.Host, joinText.text, runnerManager.testRoomIndex);
    }
}
