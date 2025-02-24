using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using TMPro;
using Steamworks;
using System;
using WebSocketSharp;
using static PlayerManager;

public class GameManager : NetworkBehaviour//PunCallbacks, IPunObservable
{
    //public Vector2Int testTime;, not part
    public GlobalEvent testevent;
    // gamePhase 0 = initialize game/assign roles 1 = main game 2 = results screen, not part
    public int gamePhase = 0;
    [Header("Game Variables")]
    //public Player campLeader;
    public PlayerRef[] cultists = new PlayerRef[] { };
    public PlayerRef[] alivePlayers = new PlayerRef[] { }; // doesn't update with alive players yet
    public Dictionary<string, Position> playerPositions = new Dictionary<string, Position>();
    //public Dictionary<Player, string> chosenBuildings = new Dictionary<Player, string>();
    public float gameTime = 0f;
    public float currentPeriod;
    public int currentDay = 0;
    public bool timeStopped = false;
    int previousDay = -1;
    [Header("Game Settings")]
    // When an index of this is true, a cultist is added when the players playing is equal to that number.
    public bool[] cultistAssignment = new bool[] { };
    public float hourLength = 60f;
    public float timeSpeed = 1f;
    public int startCurrency = 100;
    public Vector2Int startTime;
    public float buildingChooseTimer = 8f;
    [Header("Day/Night Cycle")]
    public float timeSkipPeriod = 21f;
    public float timeSkippedPeriod = 4.5f;
    public float dayStartPeriod = 7.5f;
    [Header("Constants")]
    public string[] days;
    public string[] positions;
    //PhotonView view;
    RoleRevealer rv;
    RoomManager rm;
    PlayerManager pm;
    ScheduleManager sm;
    CameraManager cm;
    bool skippedNight = false;
    bool startedDay = false;
    GameTimer gt;
    public GameEvent OnTimeChange;
    public RevealRoles OnRevealRoles;
    public GameEvent OnChangeDay;
    public GameEvent OnUpdatePositions;
    public GameEvent OnNightSkipStart;
    public GameEvent OnNightSkip;
    public GameEvent OnDayStart;
    public GameEvent OnTimeStop;
    public GameEvent OnTimeResume;
    public delegate void GameEvent();
    public delegate void RevealRoles(bool isCultist);
    bool init = false;

    public enum Position
    {
        Habitant = 0,
        Guard = 1,
        Leader = 2,
    }

    // Open when need new variable to synchronize
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        stream.SendNext(gamePhase);
    //        stream.SendNext(cultists);
    //        stream.SendNext(gameTime);
    //        stream.SendNext(timeSpeed);
    //    }
    //    else
    //    {
    //        gamePhase = (int)stream.ReceiveNext();
    //        cultists = (Player[])stream.ReceiveNext();
    //        gameTime = (float)stream.ReceiveNext();
    //        timeSpeed = (float)stream.ReceiveNext();
    //    }
    //}
    
    private void Awake()
    {
        rv = gameObject.GetComponent<RoleRevealer>();
        //view = transform.GetComponent<PhotonView>();
        OnRevealRoles += rv.RevealRole;
        rm = FindFirstObjectByType<RoomManager>();
        pm = FindFirstObjectByType<PlayerManager>();
        sm = FindFirstObjectByType<ScheduleManager>();
        gt = gameObject.GetComponent<GameTimer>();
        cm = FindFirstObjectByType<CameraManager>();
        if (!SessionData.isTesting)
        {
            FindFirstObjectByType<BlackScreen>().ShowCover();
        }
    }

    public override void Spawned()
    {
        init = true;
    }

    // Assigns rooms to different players, sohuld only be called on state authority
    void AssignRooms()
    {
       int playerCount = Runner.ActivePlayers.Count();
       if (Runner.ActivePlayers.Count() > rm.playerRooms.Count)
        {
            Debug.LogError("Not enough rooms!");
            // Stop game function here
            return;
        }

        int[] roomAssignment = new int[playerCount];

        for (int i = 0; i < roomAssignment.Length; i++) roomAssignment[i] = -1;

        for (int i = 0; i < roomAssignment.Length; i++)
        {
            int randomRoom = UnityEngine.Random.Range(0, rm.playerRooms.Count);
            while (roomAssignment.Contains(randomRoom))
            {
                randomRoom = UnityEngine.Random.Range(0, rm.playerRooms.Count);
            }
            roomAssignment[i] = randomRoom;
        }
        
        for (int i = 0; i < roomAssignment.Length; i++)
        {
            PlayerRef rPlayer = new List<PlayerRef>(Runner.ActivePlayers)[i];
            pm.playerProperties[rPlayer].SetRoom(roomAssignment[i]);
        }
    }

    void InitiatePositions() // Initiates the positions for every player in the lobby, gets removed on death
    {
        //foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        //{
        //    view.RPC("CreatePositionToken", RpcTarget.AllBuffered, player.CustomProperties["name"], (int)Position.Habitant);
        //}
    }

    void SetCurrency()
    {
        // Sets  the currency to the start currency for every player in the lobby
        foreach (PlayerRef player in  Runner.ActivePlayers)
        {
            pm.playerProperties[player].SetCurrency(startCurrency);
        }
    }

    private void Update()
    {
        if (!init) return;
        CheckDay();
        UpdateGameTime();
        if (!Runner.IsServer) return;
        //if (Input.GetKeyDown(KeyCode.Backspace)) SetTime(testTime.x, testTime.y);
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            string[] newStrings = new string[] { testevent.name };
            float[] newTimes = new float[] { testevent.time };
            float[] newLengths = new float[] { testevent.length };
            bool[] newCultistEvents = new bool[] { testevent.cultistEvent };
            //PhotonNetwork.OpCleanRpcBuffer(sm.GetComponent<PhotonView>());
            // Filter cultists events when making function
            //sm.GetComponent<PhotonView>().RPC("AddGlobalEvents", RpcTarget.AllBuffered, (object)newStrings, (object)newTimes, (object)newLengths, (object)newCultistEvents);
        }
        PhaseProperties();
        CheckNightSkip();
        CheckDayStart();
    }

    void CheckDay()
    {
        if (previousDay != currentDay)
        {
            previousDay = currentDay;
            OnChangeDay?.Invoke();
        }
    }

    void PhaseProperties()
    {
        switch (gamePhase)
        {
            case 0:
                gamePhase = 1;
                Phase0();
                break;
            default:
                break;
        }
    }

    void Phase0()
    {
        CreatePlayerProperties();
        AssignRooms(); // Sets the room properties of each player
        SetCurrency(); // Sets the default currency of each player
        SpawnPositions(); // Spawns each player
        AssignRoles(); // Assigns the roles of each player (and reveals)
        SetTime(startTime.x, startTime.y);
    }

    void CreatePlayerProperties()
    {
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            pm.playerProperties.Add(player, new PlayerProperties("", false, -1, 0));
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendRole([RpcTarget] PlayerRef player, bool isCultist)
    {
        pm.currentPlayerProperties.SetIsCultist(isCultist);
        OnRevealRoles?.Invoke(isCultist);
    }

    void CheckDayStart()
    {
        if (startedDay) return;
        if (currentPeriod - (currentDay * 24f) > dayStartPeriod)
        {
            startedDay = true;
            //view.RPC("DayStartSequence", RpcTarget.All);
        }
    }

    //[PunRPC]
    public void DayStartSequence()
    {
        OnDayStart?.Invoke();
    }

    void CheckNightSkip()
    {
        if (skippedNight) return;
        if (currentPeriod - (currentDay * 24f) > timeSkipPeriod)
        {
            NightSkip();
        }
    }

    void NightSkip()
    {
        skippedNight = true;
        //view.RPC("NightSkipSequence", RpcTarget.All);
    }

    //[PunRPC]
    public void NightSkipSequence()
    {
        OnNightSkipStart?.Invoke();
        gt.StartTimer(buildingChooseTimer + 1f);
        StopTime();
        gt.onTimerFinish.AddListener(SetNightTime);
        ResetChosenBuildings(); // Resets the building data for all clients
    }

    public void SetNightTime()
    {
        //if (!PhotonNetwork.IsMasterClient) return;
        skippedNight = false;
        Vector2Int newTime = PeriodToClockTime(timeSkippedPeriod);
        SetTime(newTime.x, newTime.y);
        startedDay = false;
        ResumeTime();
        TeleportToBuildings();
    }

    // Call to every client
    //[PunRPC]
    public void SetChosenBuilding(string buildingName)//, PhotonMessageInfo info) 
    {
        //Player player = info.Sender;
        string newBuilding = "";
        if (buildingName != "house")
        {
            if (Array.Find(rm.workRooms.ToArray(), room => room.roomName == buildingName) != null) // if the room is found
            {
                //if (chosenBuildings.ContainsKey(player)) chosenBuildings[player] = buildingName; // Set the chosen building
            } 
        }
    }

    //[PunRPC]
    public void NightSkipEvent()
    {
        OnNightSkip?.Invoke();
    }

    void TeleportToBuildings()
    {
        //foreach (KeyValuePair<Player, string> pair in  chosenBuildings)
        //{
        //    Transform tpTransform = null;
        //    if (pair.Value == "house" || pair.Value.IsNullOrEmpty())
        //    {
        //        tpTransform = rm.playerRooms[(int)pair.Key.CustomProperties["room"]].spawnTransform;
        //        Debug.Log("house set");
        //    }
        //    else
        //    {
        //        tpTransform = rm.GetWorkBuilding(pair.Value).spawnTransform;
        //    }
        //    Debug.Log(tpTransform.position);
        //    pm.photonView.RPC("Teleport", pair.Key, tpTransform.position, tpTransform.rotation);
        //}
        //view.RPC("NightSkipEvent", RpcTarget.All);
    }

    void ResetChosenBuildings()
    {
        //chosenBuildings.Clear();
        //foreach (Player player in alivePlayers)
        //{
        //    chosenBuildings.Add(player, "house"); // Adds the player's home as the default building
        //}
    }

    // Old code for position creation, ignore
    //[PunRPC]
    public void CreatePositionToken(string player, int position)
    {
        playerPositions.Add(player, (Position)position);
        OnUpdatePositions?.Invoke();
    }

    //[PunRPC]
    public void RemovePositionToken(string player)
    {
        if (!playerPositions.ContainsKey(player)) return;
        playerPositions.Remove(player);
        OnUpdatePositions?.Invoke();
    }

    //[PunRPC]
    public void ModifyPositionToken(string player, int position)
    {
        if (!playerPositions.ContainsKey(player)) return;
        playerPositions[player] = (Position)position;
    }

    void AssignRoles()
    {
        List<PlayerRef> players = new List<PlayerRef>();
        // Sets isCultist to false for every player, adds to playerlist
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            AssignRole(player, false);
            players.Add(player);
        }
        alivePlayers = players.ToArray();

        // Sets cultistNumber to the amount of cultists in the game
        int cultistNumber = 0;
        int i = 0;
        while (i < Runner.ActivePlayers.Count())
        {
            if (cultistAssignment[i] == true) cultistNumber++;
            i++;
        }
        //Debug.Log("Cultist number " + cultistNumber);

        // Creates a list of the current cultists
        List<PlayerRef> assignedCultists = new List<PlayerRef>(Runner.ActivePlayers);
        Debug.Log("Player list count: " + assignedCultists.Count);
        while (assignedCultists.Count > cultistNumber)
        {
            int removalIndex = UnityEngine.Random.Range(0, assignedCultists.Count);
            Debug.Log("Removing at " + removalIndex);
            assignedCultists.RemoveAt(removalIndex);
        }

        // Sets isCultist to true for every cultist
        foreach (PlayerRef cultist in assignedCultists)
        {
            AssignRole(cultist, true);
        }

        cultists = assignedCultists.ToArray();

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            RPC_SendRole(player, pm.playerProperties[player].isCultist);
        }
    }

    void AssignRole(PlayerRef player, bool isCultist)
    {
        pm.playerProperties[player].SetIsCultist(isCultist);
    }

    public void SpawnPositions()
    {
        // Invokes reveal roles delegate for the role reveal sequence
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            Transform roomT = rm.playerRooms[pm.playerProperties[player].room].spawnTransform;
            pm.SpawnPlayerAtTransform(Runner, player, roomT);
        }
        //OnRevealRoles?.Invoke((bool)PhotonNetwork.LocalPlayer.CustomProperties["isCultist"]);
    }

    void UpdateGameTime()
    {
        if (timeStopped) return;
        gameTime += Time.deltaTime * timeSpeed;
        currentDay = Mathf.FloorToInt((gameTime + hourLength) / (hourLength * 24f));
        currentPeriod = gameTime / hourLength;
    }

    /// <summary>
    /// Skips time forward to the specified time, only available for master client
    /// </summary>
    /// <param name="hour">Hour of the clock, number from 1 to 24</param>
    /// <param name="minute">Minute of the clock, number from 0 to 59</param>
    public void SetTime(int hour, int minute = 0)
    {
        //if (!PhotonNetwork.IsMasterClient) return;
        if (hour < 1 || hour > 24) return;
        if (minute < 0 || minute > 59) return;

        float timeAdd;
        int r = Mathf.FloorToInt(gameTime / (hourLength * 24f));
        Vector2Int clockTime = PeriodToClockTime((Mathf.FloorToInt(currentPeriod) - (r * 24)) + ((gameTime - (Mathf.FloorToInt(currentPeriod) * hourLength)) / hourLength));
        //Debug.Log(clockTime);

        // Time add hours
        int currentHour = clockTime.x;
        timeAdd = (hour - (currentHour + 1)) * hourLength;

        // Time add minutes
        int currentMinute = clockTime.y;
        float minuteLength = hourLength / 60f;
        int minDiff = minute - currentMinute;
        timeAdd += minDiff * minuteLength;

        // if time add is negative, cycle the day
        if (timeAdd < 0f) timeAdd += hourLength * 24f;
        gameTime += timeAdd;

        OnTimeChange?.Invoke();
    }

    /// <summary>
    /// Converts period time to clock itme
    /// </summary>
    /// <param name="periodTime"></param>
    /// <returns>Vector2Int(hour (0 - 23), minute (0-59))</returns>
    public Vector2Int PeriodToClockTime(float periodTime)
    {
        int roundedPeriod = Mathf.FloorToInt(periodTime);
        float periodProgress = periodTime - roundedPeriod;
        while (roundedPeriod > 23) roundedPeriod -= 24;
        int currentMinute = Mathf.FloorToInt(periodProgress * 60f);
        return new Vector2Int(roundedPeriod, currentMinute);
    }

    public void StopTime()
    {
        //if (!PhotonNetwork.IsMasterClient) return;
        if (timeStopped) return;
        timeStopped = true;
        OnTimeStop?.Invoke();
    }

    public void ResumeTime()
    {
        //if (!PhotonNetwork.IsMasterClient) return;
        if (!timeStopped) return;
        timeStopped = false;
        OnTimeResume?.Invoke();
    }

    /// <summary>
    /// Converts period time to a clock string
    /// </summary>
    /// <param name="periodTime"></param>
    /// <returns></returns>
    public string PeriodToClockString(float periodTime)
    {
        Vector2Int clockTimeStart = PeriodToClockTime(periodTime);
        string startMins = clockTimeStart.y.ToString();
        if (startMins.Length == 1) startMins = "0" + startMins;
        string startMeridiem = "AM";
        if (clockTimeStart.x > 10 && clockTimeStart.x != 23) startMeridiem = "PM";
        clockTimeStart.x++;
        if (clockTimeStart.x > 12) clockTimeStart.x -= 12;
        return $"{clockTimeStart.x}:{startMins} {startMeridiem}";
    }

    public string GetDay(int day)
    {
        if (day < 0)
        {
            return days[0];
        }
        if (day > days.Length-1) day = day - (Mathf.FloorToInt(day / days.Length) * days.Length);
        return days[day];
    }

    public float GetDayProgress()
    {
        return (gameTime - hourLength * 24f * currentDay) / (hourLength * 24f);
    }

    //public override void OnPlayerLeftRoom(Player otherPlayer)
    //{
    //    string name = (string)otherPlayer.CustomProperties["name"];
    //    if (name == null) return;
    //    if (!PhotonNetwork.IsMasterClient) return;
    //    view.RPC("RemovePositionToken", RpcTarget.AllBuffered, name);
    //    if (cultists.Contains(otherPlayer))
    //    {
    //        List<Player> newCultists = cultists.ToList();
    //        newCultists.Remove(otherPlayer);
    //        cultists = newCultists.ToArray();
    //        CheckWinCondition();
    //    }
    //    if (alivePlayers.Contains(otherPlayer))
    //    {
    //        List<Player> newPlayers = alivePlayers.ToList();
    //        newPlayers.Remove(otherPlayer);
    //        alivePlayers = newPlayers.ToArray();
    //        CheckWinCondition();
    //    }
    //}

    public void CheckWinCondition()
    {
        //if (cultists.Length == 0)
        //{
        //    // Inno win condition
        //    Debug.LogError("Innos win!");
        //}
    }
}
