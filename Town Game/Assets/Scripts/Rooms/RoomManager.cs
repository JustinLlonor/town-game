using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
using System;
//using Photon.Realtime;
using System.Linq;
using Steamworks;

public class RoomManager : MonoBehaviour
{
    public string testRoom;
    public List<MapRoom> playerRooms = new List<MapRoom>();
    public List<MapRoom> workRooms = new List<MapRoom>();
    [Header("Building Choosing Sequence")]
    public Transform buildingCameraTransform;
    public float buildingTransitionSpeed = 10f;
    [SerializeField] List<MapRoom> ownedRooms = new List<MapRoom>();
    [SerializeField] int currentBuilding = 0;
    CameraManager cm;
    GameManager gm;
    bool buildingsChosen = false; // If the building choosing sequence is happening

    private void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
        cm = FindFirstObjectByType<CameraManager>();
        foreach (Transform child in transform)
        {
            MapRoom.RoomType type = child.GetComponent<MapRoom>().roomType;
            if (type == MapRoom.RoomType.Living)
            {
                playerRooms.Add(child.GetComponent<MapRoom>());
            }
            if (type == MapRoom.RoomType.Work)
            {
                workRooms.Add(child.GetComponent<MapRoom>());
            }
        }
    }

    private void Start()
    {
        gm.OnNightSkipStart += BuildingChooseStart;
        gm.OnNightSkip += BuildingChooseEnd;
    }

    private void Update()
    {
        // Debug inputs
        //if (Input.GetKeyDown(KeyCode.U)) AddWorker(PhotonNetwork.LocalPlayer, testRoom); // test
        if (Input.GetKeyDown(KeyCode.RightArrow)) ScrollRight();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ScrollLeft();
        if (Input.GetKeyDown(KeyCode.DownArrow)) ChooseBuilding();
        UpdateCamPosition();
    }

    //[PunRPC]
    //public void AddWorker(Photon.Realtime.Player player, string roomName)
    //{
    //    int index = Array.FindIndex(workRooms.ToArray(), room => room.name == roomName);
    //    if (index == -1) return;

    //    if (!workRooms[index].workers.Contains(player))
    //    {
    //        workRooms[index].workers.Add(player);
    //        return;
    //    }
    //    Debug.LogError("Player already works at specified location!");
    //}

    //[PunRPC]
    //public void RemoveWorker(Photon.Realtime.Player player, string roomName)
    //{
    //    int index = Array.FindIndex(workRooms.ToArray(), room => room.name == roomName);
    //    if (index == -1) return;

    //    if (workRooms[index].workers.Contains(player))
    //    {
    //        workRooms[index].workers.Remove(player);
    //        return;
    //    }
    //}

    /// <summary>
    /// Building start seqeunce
    /// </summary>
    void BuildingChooseStart()
    {
        //if (!gm.alivePlayers.Contains(PhotonNetwork.LocalPlayer)) return;
        //ownedRooms = new List<MapRoom>() { playerRooms[(int)PhotonNetwork.LocalPlayer.CustomProperties["room"]] }; // Creates new owned rooms list
        foreach (MapRoom room in workRooms)
        {
            //if (room.workers.Contains(PhotonNetwork.LocalPlayer)) ownedRooms.Add(room);
        }
        // Sets the default hovered/selected building to the player's house
        currentBuilding = 0;
        buildingCameraTransform.position = ownedRooms[0].viewTransform.position;
        ChooseBuilding();

        cm.trackedCinematicTransform = buildingCameraTransform;
        cm.StartModeTransition(1f, CameraManager.CameraMode.Cinematic);

        buildingsChosen = true;
    }

    void ScrollRight()
    {
        currentBuilding++;
        if (currentBuilding >= ownedRooms.Count)
        {
            currentBuilding = 0; 
        }
    }

    void ScrollLeft()
    {
        currentBuilding--;
        if (currentBuilding <= -1)
        {
            currentBuilding = ownedRooms.Count - 1;
        }
    }

    void UpdateCamPosition()
    {
        if (!buildingsChosen) return;
        if (ownedRooms.Count == 0) return;
        Transform newTransform = ownedRooms[currentBuilding].viewTransform;

        buildingCameraTransform.position = Vector3.Lerp(buildingCameraTransform.position, newTransform.position, Time.deltaTime * buildingTransitionSpeed);
    }

    /// <summary>
    /// Chooses the building, resets player energy if the building is house (do it on master client)
    /// </summary>
    void ChooseBuilding()
    {
        string sentBuilding = "house";
        if (currentBuilding != 0)
        {
            Debug.Log(currentBuilding);
            sentBuilding = ownedRooms[currentBuilding].roomName;
        }
        Debug.Log(sentBuilding);
        //gm.photonView.RPC("SetChosenBuilding", RpcTarget.All, sentBuilding);
    }

    void BuildingChooseEnd()
    {
        buildingsChosen = false;
        cm.StartModeTransition(1f, CameraManager.CameraMode.FirstPerson);
    }

    public MapRoom GetWorkBuilding(string roomName)
    {
        return Array.Find(workRooms.ToArray(), room => room.name == roomName);
    }
}
