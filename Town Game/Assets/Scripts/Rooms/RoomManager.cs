using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
    public AnimationCurve buildingTransitionCurve;
    private float buildingTransitionProgress = 0f;
    private Vector3 buildingTransitionStart;
    [SerializeField] List<MapRoom> ownedRooms = new List<MapRoom>();
    [SerializeField] int currentBuilding = 0;
    CameraManager cm;
    GameManager gm;
    PlayerManager pm;
    InputManager inputManager;
    bool buildingsChosen = false; // If the building choosing sequence is happening

    private void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
        cm = FindFirstObjectByType<CameraManager>();
        pm = FindFirstObjectByType<PlayerManager>();
        inputManager = FindFirstObjectByType<InputManager>();
        foreach (Transform child in transform)
        {
            RoomCategory type = child.GetComponent<MapRoom>().roomCategory;
            if (type == RoomCategory.House)
            {
                playerRooms.Add(child.GetComponent<MapRoom>());
            }
        }
    }

    private void Start()
    {
        gm.OnNightSkipStart += BuildingChooseStart;
        gm.OnNightSkipEnd += BuildingChooseEnd;
        inputManager.onScrollRight += ScrollRight;
        inputManager.onScrollLeft += ScrollLeft;
        inputManager.onChooseBuilding += ChooseBuilding;
    }

    private void Update()
    {
        // Debug inputs
        //if (Input.GetKeyDown(KeyCode.U)) AddWorker(PhotonNetwork.LocalPlayer, testRoom); // test
        UpdateCamPosition();
    }

    /// <summary>
    /// Building start seqeunce
    /// </summary>
    void BuildingChooseStart()
    {
        //if (!gm.alivePlayers.Contains(PhotonNetwork.LocalPlayer)) return;
        ownedRooms = new List<MapRoom>() { playerRooms[pm.currentPlayerProperties.room] }; // Creates new owned rooms list
        //TODO: unlocked rooms add
        foreach (MapRoom room in workRooms)
        {
            ownedRooms.Add(room);
            Debug.Log("Added " + room.roomName);
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
        ResetBuildingTransition();
        if (currentBuilding >= ownedRooms.Count)
        {
            currentBuilding = 0; 
        }
    }

    void ScrollLeft()
    {
        currentBuilding--;
        ResetBuildingTransition();
        if (currentBuilding <= -1)
        {
            currentBuilding = ownedRooms.Count - 1;
        }
    }

    void ResetBuildingTransition()
    {
        buildingTransitionStart = buildingCameraTransform.position;
        buildingTransitionProgress = 0f;
    }

    void UpdateCamPosition()
    {
        if (!buildingsChosen) return;
        if (ownedRooms.Count == 0) return;
        Transform newTransform = ownedRooms[currentBuilding].viewTransform;

        buildingTransitionProgress += Time.deltaTime * buildingTransitionSpeed;
        buildingTransitionProgress = Mathf.Clamp01(buildingTransitionProgress);
        float time = buildingTransitionCurve.Evaluate(buildingTransitionProgress);
        buildingCameraTransform.position = Vector3.Lerp(buildingTransitionStart, newTransform.position, time);
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
        // Sends the chosen building to the server
        pm.currentPlayer.GetComponent<PlayerRoomChoose>().RPC_ChooseBuilding(sentBuilding);
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
