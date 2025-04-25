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
    public GameObject buildingChooseUI;
    public float buildingTransitionSpeed = 10f;
    public AnimationCurve buildingTransitionCurve;
    public Selection onSelectionUpdate;
    public int houseEnergyGain = 1;
    private float buildingTransitionProgress = 0f;
    private Vector3 buildingTransitionStart;
    [SerializeField] List<MapRoom> ownedRooms = new List<MapRoom>();
    [SerializeField] int currentBuilding = 0;
    CameraManager cm;
    GameManager gm;
    PlayerManager pm;
    InputManager inputManager;
    CursorManager cursorManager;
    bool buildingsChosen = false; // If the building choosing sequence is happening
    int chosenBuilding = 0;

    public delegate void Selection(string roomName, int energyDiff, bool canAfford, bool selected);

    private void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
        cm = FindFirstObjectByType<CameraManager>();
        pm = FindFirstObjectByType<PlayerManager>();
        inputManager = FindFirstObjectByType<InputManager>();
        cursorManager = FindFirstObjectByType<CursorManager>();
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
        gm.OnNightSkipEnd += DisableChooseUI;
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
        Debug.Log("starting");
        //if (!gm.alivePlayers.Contains(PhotonNetwork.LocalPlayer)) return;
        ownedRooms = new List<MapRoom>() { playerRooms[pm.currentPlayerProperties.room] }; // Creates new owned rooms list
        foreach (MapRoom room in workRooms)
        {
            ownedRooms.Add(room);
            Debug.Log("Added " + room.roomName);
            //if (room.workers.Contains(PhotonNetwork.LocalPlayer)) ownedRooms.Add(room);
        }
        // Sets the default hovered/selected building to the player's house
        currentBuilding = 0;
        chosenBuilding = 0;
        buildingCameraTransform.position = ownedRooms[0].viewTransform.position;
        ChooseBuilding();

        cm.trackedCinematicTransform = buildingCameraTransform;
        cm.StartModeTransition(1f, CameraManager.CameraMode.Cinematic);
        StartCoroutine(WaitEnableChooseUI());

        buildingsChosen = true;
    }

    public void ScrollRight()
    {
        currentBuilding++;
        if (currentBuilding >= ownedRooms.Count)
        {
            currentBuilding = 0; 
        }
        ResetBuildingTransition();

    }

    public void ScrollLeft()
    {
        currentBuilding--;
        if (currentBuilding <= -1)
        {
            currentBuilding = ownedRooms.Count - 1;
        }
        ResetBuildingTransition();

    }

    void ResetBuildingTransition()
    {
        buildingTransitionStart = buildingCameraTransform.position;
        buildingTransitionProgress = 0f;
        HoverBuilding();
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
    public void ChooseBuilding()
    {
        string sentBuilding = "house";
        int energyDiff = houseEnergyGain;
        if (currentBuilding != 0)
        {
            sentBuilding = ownedRooms[currentBuilding].roomName;
            energyDiff = ownedRooms[currentBuilding].energyDiff;
        }
        onSelectionUpdate?.Invoke(sentBuilding, energyDiff, true, true);
        // Sends the chosen building to the server
        chosenBuilding = currentBuilding;
        pm.currentPlayer.GetComponent<PlayerRoomChoose>().RPC_ChooseBuilding(sentBuilding);
    }

    private void HoverBuilding()
    {
        string sentBuilding = "house";
        int energyDiff = houseEnergyGain;
        if (currentBuilding != 0)
        {
            sentBuilding = ownedRooms[currentBuilding].roomName;
            energyDiff = ownedRooms[currentBuilding].energyDiff;
        }
        onSelectionUpdate?.Invoke(sentBuilding, energyDiff, true, chosenBuilding == currentBuilding); // selected if the hovered building is the current
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

    IEnumerator WaitEnableChooseUI()
    {
        yield return new WaitForSeconds(1f);
        EnableChooseUI();
        onSelectionUpdate?.Invoke("house", houseEnergyGain, true, true);
        cursorManager.Unlock();
    }

    private void EnableChooseUI()
    {
        buildingChooseUI.SetActive(true);
    }

    private void DisableChooseUI()
    {
        buildingChooseUI.SetActive(false);
    }
}
