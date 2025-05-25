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

public class GameManager : NetworkBehaviour
{
    // gamePhase 0 = initialize game/assign roles 1 = main game 2 = results screen, not part
    public int gamePhase = 0;
    [Header("Game Variables")]
    public PlayerRef leader;
    public PlayerRef[] cultists = new PlayerRef[] { };
    [Networked] public int cultistsLeft { get; set; }
    public List<PlayerRef> alivePlayers = new List<PlayerRef>(); // doesn't update with alive players yet
    public Dictionary<string, Position> playerPositions = new Dictionary<string, Position>();
    public Dictionary<PlayerRef, string> chosenBuildings = new Dictionary<PlayerRef, string>();
    [Networked] public float gameTime { get; set; } = 0f;
    public float currentPeriod;
    public int currentDay = 0;
    [Networked] public bool timeStopped { get; set; } = false;
    int previousDay = -1;
    [Header("Game Settings")]
    // When an index of this is true, a cultist is added when the players playing is equal to that number.
    public bool[] cultistAssignment = new bool[] { };
    public float hourLength = 60f;
    public float timeSpeed = 1f;
    public int startCurrency = 100;
    public int startEnergy = 2;
    /// <summary>
    /// The maximum amount of energy the player is allowed to have
    /// </summary>
    public int maxEnergy = 2;
    public Vector2Int startTime;
    public float buildingChooseTimer = 8f;
    [Header("Day/Night Cycle")]
    public float timeSkipPeriod = 21f;
    public float timeSkippedPeriod = 4.5f;
    public float dayStartPeriod = 7.5f;
    [Header("Constants")]
    public string[] days;
    public string[] positions;
    RoleRevealer rv;
    RoomManager rm;
    PlayerManager pm;
    ScheduleManager sm;
    RunnerManager runnerManager;
    PositionManager positionManager;
    [Networked] public bool skippedNight { get; set; } = false;
    bool previousSkippedNight = false;
    bool startedDay = false;
    public GameEvent OnTimeChange;
    public RevealRoles OnRevealRoles;
    /// <summary>
    /// Invoked when the day changes. Also invoked at the start of the game
    /// </summary>
    public GameEvent OnChangeDay;
    public GameEvent OnUpdatePositions;
    public GameEvent OnNightSkipStart;
    public GameEvent OnNightSkipEnd;
    public GameEvent OnDayStart;
    public GameEvent OnTimeStop;
    public GameEvent OnTimeResume;
    public Timer onUpdateNightTimer;
    public delegate void GameEvent();
    public delegate void RevealRoles(bool isCultist);
    public delegate void Timer(float timer);
    [Networked] public TickTimer nightTimer { get; set; }
    public bool init = false;

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
        OnRevealRoles += rv.RevealRole;
        rm = FindFirstObjectByType<RoomManager>();
        pm = FindFirstObjectByType<PlayerManager>();
        sm = FindFirstObjectByType<ScheduleManager>();
        positionManager = FindAnyObjectByType<PositionManager>();
        runnerManager = FindFirstObjectByType<RunnerManager>();
        runnerManager.onPlayerLeave += RemoveAlivePlayer;
        //cm = FindFirstObjectByType<CameraManager>();
        if (!SessionData.isTesting)
        {
            FindFirstObjectByType<BlackScreen>().ShowCover();
        }
    }

    void RemoveAlivePlayer(PlayerRef player)
    {
        alivePlayers.Remove(player);
    }

    public override void Spawned()
    {
        InputManager im = FindFirstObjectByType<InputManager>();
        // Sets the input maps to building choose when night starts, and sets to base when night skip ends
        OnNightSkipStart += im.SetCurrentToBuildingChoose;
        OnNightSkipEnd += im.SetCurrentToBase;
        init = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (alivePlayers.Count < pm.playerObjects.Count)
        {
            pm.removePlayers = true;
            Debug.Log("setting");
        } // More alive players than there are playe robjects
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
            pm.SetRoom(rPlayer, roomAssignment[i]);
        }
    }

    /// <summary>
    /// Sets the default properties of all players
    /// </summary>
    void SetProperties()
    {
        // Sets  the currency to the start currency for every player in the lobby
        foreach (PlayerRef player in  Runner.ActivePlayers)
        {
            pm.SetMoney(player, startCurrency);
            //pm.playerProperties[player].SetCurrency(startCurrency);
            pm.SetEnergy(player, startEnergy);
            //pm.playerProperties[player].SetEnergy(startEnergy);
        }
    }

    private void Update()
    {
        if (!init) return;
        CheckDay(); 
        UpdateGameTime();
        ClientNightTimer();
        ClientNightSkip();
        CheckDayStart();
        if (!Runner.IsServer) return;
        //if (Input.GetKeyDown(KeyCode.Backspace)) SetTime(testTime.x, testTime.y);
        PhaseProperties();
        CheckNightSkip();
        CheckNightTimer();
    }

    void ClientNightSkip()
    {
        if (previousSkippedNight != skippedNight)
        {
            previousSkippedNight = skippedNight;
            if (skippedNight) NightSkipSequence();
            else NightSkipEvent();
        }
    }

    void ClientNightTimer()
    {
        if (!skippedNight) return;
        float remainingTime = 0;
        if (nightTimer.RemainingTime(Runner) != null)
        {
            remainingTime = (float)nightTimer.RemainingTime(Runner);
        }
        onUpdateNightTimer?.Invoke(remainingTime);
    }

    void CheckNightTimer()
    {
        if (!skippedNight) return;
        if (nightTimer.ExpiredOrNotRunning(Runner))
        {
            SetNightTime();
        }
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

    IEnumerator UnfreezeAll(float time)
    {
        yield return new WaitForSeconds(time);
        foreach (KeyValuePair<PlayerRef, NetworkId> kvp in pm.playerObjects)
        {
            NetworkObject player = pm.GetPlayerNetworkObject(kvp.Key);
            GameObject playerObject = player.gameObject;
            playerObject.GetComponent<PlayerMovement>().Unfreeze();
        }
    }

    void Phase0()
    {
        pm.CreatePlayerProperties(); // Initialize player properties
        AssignPlayerPositions(); // Add to player positions
        AssignRooms(); // Sets the room properties of each player
        SetProperties(); // Sets the default currency of each player
        SpawnPositions(); // Spawns each player
        AssignRoles(); // Assigns the roles of each player (and reveals)
        SetTime(startTime.x, startTime.y);
    }

    void AssignPlayerPositions()
    {
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            //pm.playerProperties.Add(player, new PlayerProperties("", false, -1, 0));
            positionManager.AddPlayerToBranch(player, 0);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendRole([RpcTarget] PlayerRef player, bool isCultist)
    {
        //pm.currentPlayerProperties.SetIsCultist(isCultist);
        OnRevealRoles?.Invoke(isCultist);
    }

    void CheckDayStart()
    {
        if (startedDay) return;
        if (currentPeriod - (currentDay * 24f) > dayStartPeriod)
        {
            startedDay = true;
            DayStartSequence();
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
        nightTimer = TickTimer.CreateFromSeconds(Runner, buildingChooseTimer + 1f);
        StopTime();
        ResetChosenBuildings();
    }

    public void NightSkipSequence()
    {
        OnNightSkipStart?.Invoke();
    }
    
    public void SetNightTime()
    {
        //if (!PhotonNetwork.IsMasterClient) return;
        skippedNight = false;
        Vector2Int newTime = PeriodToClockTime(timeSkippedPeriod);
        SetTime(newTime.x, newTime.y);
        ResumeTime();
        TeleportToBuildings();
    }

    public void SetChosenBuilding(string buildingName, PlayerRef player)//, PhotonMessageInfo info) 
    {
        if (buildingName != "house" && positionManager.PlayerHasAccessToRoom(player, buildingName))
        {
            MapRoom foundRoom = Array.Find(rm.workRooms.ToArray(), room => room.roomName == buildingName);
            if (foundRoom != null) // if the room is found
            {
                int energyDiff = foundRoom.energyDiff;
                //int playerEnergy = pm.playerProperties[player].energy;
                int playerEnergy = pm.GetEnergy(player);
                if (playerEnergy + energyDiff < 0) return; // If the energy is invalid
                Debug.Log("successfully set to " + buildingName);
                if (chosenBuildings.ContainsKey(player)) chosenBuildings[player] = buildingName; // Set the chosen building
            }
            return;
        }
        if (chosenBuildings.ContainsKey(player)) chosenBuildings[player] = "house";
    }

    /// <summary>
    /// When the building choose sequence ends on the client
    /// </summary>
    public void NightSkipEvent()
    {
        startedDay = false;
        OnNightSkipEnd?.Invoke();
    }

    /// <summary>
    /// Teleports players to their chosen building from the chosen building list
    /// </summary>
    void TeleportToBuildings()
    {
        foreach (KeyValuePair<PlayerRef, string> pair in  chosenBuildings)
        {
            PlayerRef player = pair.Key;
            Transform tpTransform = null;
            MapRoom tpRoom = null;
            if (pair.Value == "house" || pair.Value.IsNullOrEmpty())
            {
                //tpRoom = rm.playerRooms[pm.playerProperties[player].room];
                tpRoom = rm.playerRooms[pm.GetRoom(player)];
                tpTransform = tpRoom.spawnTransform;
                Debug.Log("house set");
            }
            else
            {
                tpRoom = rm.GetWorkBuilding(pair.Value);
                int energyDiff = tpRoom.energyDiff;
                //int playerEnergy = pm.playerProperties[player].energy;
                int playerEnergy = pm.GetEnergy(player);
                if (energyDiff + playerEnergy < 0)
                {
                    //tpRoom = rm.playerRooms[pm.playerProperties[player].room];
                    tpRoom = rm.playerRooms[pm.GetRoom(player)];
                    tpTransform = tpRoom.spawnTransform;
                    Debug.Log("house alt set");
                }
                else
                {
                    tpTransform = tpRoom.spawnTransform;
                }
            }
            int fEnergyDiff = tpRoom.energyDiff;
            if (tpRoom.roomCategory == RoomCategory.House) fEnergyDiff = rm.houseEnergyGain;
            //int fPlayerEnergy = pm.playerProperties[player].energy;
            int fPlayerEnergy = pm.GetEnergy(player);

            pm.SetEnergy(player, Mathf.Clamp(fPlayerEnergy + fEnergyDiff, 0, maxEnergy));
            pm.Teleport(pair.Key, tpTransform.position, tpTransform.rotation);
        }
    }

    void ResetChosenBuildings()
    {
        chosenBuildings.Clear();
        foreach (PlayerRef player in alivePlayers)
        {
            chosenBuildings.Add(player, "house"); // Adds the player's home as the default building
            //RPC_SendEnergy(player, pm.playerProperties[player].energy);
        }
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
        alivePlayers = new List<PlayerRef>(players);

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
        cultistsLeft = cultists.Length;

        if (!SessionData.isTesting) StartCoroutine(UnfreezeAll(8f));
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            RPC_SendRole(player, pm.GetIsCultist(player));
        }
    }

    void AssignRole(PlayerRef player, bool isCultist)
    {
        pm.SetIsCultist(player, isCultist);
    }

    public void SpawnPositions()
    {
        // Invokes reveal roles delegate for the role reveal sequence
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            Transform roomT = rm.playerRooms[pm.GetRoom(player)].spawnTransform;
            GameObject playerObject = pm.SpawnPlayerAtTransform(Runner, player, roomT);
            if (!SessionData.isTesting) playerObject.GetComponent<PlayerMovement>().Freeze();
        }
        //OnRevealRoles?.Invoke((bool)PhotonNetwork.LocalPlayer.CustomProperties["isCultist"]);
    }

    void UpdateGameTime()
    {
        currentDay = Mathf.FloorToInt((gameTime + hourLength) / (hourLength * 24f));
        currentPeriod = gameTime / hourLength;
        if (timeStopped) return;
        gameTime += Time.deltaTime * timeSpeed;
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
        if (!Runner.IsServer) return;
        if (timeStopped) return;
        timeStopped = true;
        OnTimeStop?.Invoke();
    }

    public void ResumeTime()
    {
        if (!Runner.IsServer) return;
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

    /// <summary>
    /// Converts local day time to total game time
    /// </summary>
    /// <param name="localTime"></param>
    /// <returns></returns>
    public float GetGameTimeFromLocalTime(float localTime)
    {
        return localTime + (currentDay * 24f * hourLength);
    }

    /// <summary>
    /// Returns the local time of the current game time
    /// </summary>
    /// <returns></returns>
    public float GetLocalTimeFromGameTime()
    {
        return gameTime - (currentDay * 24f * hourLength);
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
        if (!init) return 0f;
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
