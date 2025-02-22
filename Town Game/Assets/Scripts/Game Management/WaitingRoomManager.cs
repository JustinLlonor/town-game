using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitingRoomManager : MonoBehaviour//PunCallbacks, IPunObservable
{
    // 0 = not ready 1 = ready 2 = starting
    public int roomPhase = 0;
    public int playersRequired = 2;
    public int mapIndex = 2;
    // Objects that correspond to a gamephase
    public GameObject[] phaseUI;
    public GameObject hostUI;
    NetworkRunner networkRunner;

    void Awake()
    {
        networkRunner = FindFirstObjectByType<NetworkRunner>();
    }

    public void LaunchGame()
    {
        networkRunner.LoadScene(SceneRef.FromIndex(2), LoadSceneMode.Single);
    }

    void Update()
    {
        PhaseUI();
    }

    void PhaseUI()
    {
        if (!networkRunner.IsServer) return;
        hostUI.SetActive(networkRunner.ActivePlayers.Count() >= playersRequired);
        if (Input.GetKeyDown(KeyCode.T))
        {
            LaunchGame();
        }
    }
}   
