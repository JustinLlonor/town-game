using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // gamePhase 0 = initialize game/assign roles 1 = main game 2 = results screen
    public int gamePhase = 0;
    [Header("Game Variables")]
    public Player campLeader;
    public Player[] cultists = new Player[] { };
    public float gameTime = 0f;
    public int currentPeriod;
    public int currentDay = 0;
    int previousDay = -1;
    [Header("Game Settings")]
    // When an index of this is true, a cultist is added when the players playing is equal to that number.
    public bool[] cultistAssignment = new bool[] { };
    public float hourLength = 60f;
    PhotonView view;
    RoleRevealer rv;
    RoomManager rm;
    PlayerManager pm;

    public TimeChange OnTimeChange;
    public RevealRoles OnRevealRoles;
    public ChangeDay OnChangeDay;
    public delegate void TimeChange();
    public delegate void RevealRoles(bool isCultist);
    public delegate void ChangeDay();

    // Open when need new variable to synchronize
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gamePhase);
            stream.SendNext(cultists);
            stream.SendNext(gameTime);
        }
        else
        {
            gamePhase = (int)stream.ReceiveNext();
            cultists = (Player[])stream.ReceiveNext();
            gameTime = (float)stream.ReceiveNext();
        }
    }
    
    private void Awake()
    {
        rv = gameObject.GetComponent<RoleRevealer>();
        view = transform.GetComponent<PhotonView>();
        OnRevealRoles += rv.RevealRole;
        rm = FindObjectOfType<RoomManager>();
        pm = FindObjectOfType<PlayerManager>();
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient) AssignRooms();
    }
    
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        UpdateGameTime();
        CheckDay();
        PhaseProperties();
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
        AssignRoles();
        SetTime(7, 30);
    }

    void AssignRooms()
    {
        if (PhotonNetwork.PlayerList.Length > rm.playerRooms.Count)
        {
            Debug.LogError("Not enough rooms!");
            // Stop game function here
            return;
        }

        int[] roomAssignment = new int[PhotonNetwork.PlayerList.Length];

        for (int i = 0; i < roomAssignment.Length; i++) roomAssignment[i] = -1;

        for (int i = 0; i < roomAssignment.Length; i++)
        {
            int randomRoom = Random.Range(0, rm.playerRooms.Count);
            while (roomAssignment.Contains(randomRoom))
            {
                randomRoom = Random.Range(0, rm.playerRooms.Count);
            }
            roomAssignment[i] = randomRoom;
        }

        for (int i = 0; i < roomAssignment.Length; i++)
        {
            ExitGames.Client.Photon.Hashtable pProperties = PhotonNetwork.PlayerList[i].CustomProperties;
            pProperties["room"] = roomAssignment[i];
            PhotonNetwork.PlayerList[i].SetCustomProperties(pProperties);
        }
    }

    void AssignRoles()
    {
        // Sets isCultist to false for every player
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AssignRole(player, false);
        }

        // Sets cultistNumber to the amount of cultists in the game
        int cultistNumber = 0;
        int i = 0;
        while (i < PhotonNetwork.PlayerList.Length)
        {
            if (cultistAssignment[i] == true) cultistNumber++;
            i++;
        }
        Debug.Log("Cultist number " + cultistNumber);

        // Creates a list of the current cultists
        List<Player> assignedCultists = PhotonNetwork.PlayerList.ToList();
        Debug.Log("Player list count: " + assignedCultists.Count);
        while (assignedCultists.Count > cultistNumber)
        {
            int removalIndex = Random.Range(0, assignedCultists.Count);
            Debug.Log("Removing at " + removalIndex);
            assignedCultists.RemoveAt(removalIndex);
        }

        // Sets isCultist to true for every cultist
        foreach (Player cultist in assignedCultists)
        {
            AssignRole(cultist, true);
        }

        // Reveals roles
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            view.RPC("RevealRole", player);
        }

        cultists = assignedCultists.ToArray();
    }

    void AssignRole(Player player, bool isCultist)
    {
        ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;
        playerProperties["isCultist"] = isCultist;
        player.SetCustomProperties(playerProperties);
    }

    [PunRPC]
    public void RevealRole()
    {
        // Invokes reveal roles delegate for the role reveal sequence
        Transform roomT = rm.playerRooms[(int)PhotonNetwork.LocalPlayer.CustomProperties["room"]].spawnTransform;
        pm.Teleport(roomT.position, roomT.rotation);
        OnRevealRoles?.Invoke((bool)PhotonNetwork.LocalPlayer.CustomProperties["isCultist"]);
    }

    void UpdateGameTime()
    {
        gameTime += Time.deltaTime;
        currentDay = Mathf.FloorToInt((gameTime + hourLength) / (hourLength * 24f));
        currentPeriod = Mathf.FloorToInt(gameTime / hourLength);
    }

    /// <summary>
    /// Skips time forward to the specified time, only available for master client
    /// </summary>
    /// <param name="hour">Hour of the clock, number from 1 to 24</param>
    /// <param name="minute">Minute of the clock, number from 0 to 59</param>
    public void SetTime(int hour, int minute = 0)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (hour < 1 || hour > 24) return;
        if (minute < 0 || minute > 59) return;

        float timeAdd;
        int r = Mathf.FloorToInt(gameTime / (hourLength * 24f));
        Vector2Int clockTime = PeriodToClockTime((currentPeriod - (r * 24)) + ((gameTime - (currentPeriod * hourLength)) / hourLength));
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
}
