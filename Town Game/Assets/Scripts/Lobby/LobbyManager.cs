using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject loadingScreen;
    public GameObject lobby;
    public TextMeshProUGUI createText;
    public TextMeshProUGUI joinText;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreatePress()
    {
        PhotonNetwork.CreateRoom(createText.text);
    }
    public void JoinPress()
    {
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
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel(1);
    }
}
