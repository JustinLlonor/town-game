using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class UIPlayerList : MonoBehaviour//PunCallbacks
{
    // If true, players will be removed from player list on disconnect. If false, players will be marked as dead on disconnect.
    public bool removeOnDC;
    public GameObject tabPlayerPrefab;
    public Transform contentHolder;
    public Color alivePlayerColor;
    public Color deadPlayerColor;
    public Color primaryPanelColor;
    public Color secondaryPanelColor;
    List<string> containedPlayers = new List<string>();

    public ClickPlayer OnClickPlayer;
    public DeselectPlayer OnDeselectPlayer;
    public delegate void ClickPlayer(PlayerRef player);
    public delegate void DeselectPlayer(PlayerRef player);

    PlayerManager playerManager;
    RunnerManager rm;

    private void Awake()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
        rm = FindFirstObjectByType<RunnerManager>();
        rm.onPlayerJoin += PlayerEventUpdatePlayerList;
        rm.onPlayerLeave += PlayerEventUpdatePlayerList;
    }

    void PlayerEventUpdatePlayerList(PlayerRef player)
    {
        UpdatePlayerList();
    }

    // Updates player list off of players in room
    public void UpdatePlayerList()
    {
        List<GameObject> toDestroy = new List<GameObject>();
        List<PlayerRef> playerList = rm.nRunner.ActivePlayers.ToList();
        // Update individual cards, destroy unnecessary
        foreach (Transform child in contentHolder.transform)
        {
            TabPlayer tp = child.GetComponent<TabPlayer>();

            // Disconnected behaviour
            bool disconnected = true;
            foreach (PlayerRef player in playerList)
            {
                if (playerManager.playerObjects[player].GetComponent<Player>().nickname == tp.nick)
                {
                    disconnected = false;
                    break;
                }
            }
            if (disconnected)
            {
                if (removeOnDC)
                {
                    toDestroy.Add(tp.gameObject);
                }
                else
                {
                    tp.SetNameColor(deadPlayerColor);
                    tp.CrossName(true);
                }
            }
        }

        foreach (GameObject go in toDestroy) Destroy(go);

        // Add missing players
        foreach (PlayerRef player in playerList)
        {
            if (!containedPlayers.Contains(playerManager.playerObjects[player].GetComponent<Player>().nickname))
            {
                AddPlayer(player);
            }
        }

        // Have something which updates positions later

        UpdateColors();
    }

    public void AddPlayer(PlayerRef player)
    {
        string name = playerManager.playerObjects[player].GetComponent<Player>().nickname;
        GameObject newPlayer = Instantiate(tabPlayerPrefab, contentHolder);
        TabPlayer tp = newPlayer.GetComponent<TabPlayer>();
        tp.uPlayerList = this;
        tp.SetName(name);
        tp.SetNameColor(alivePlayerColor);
        tp.player = player;
        OnClickPlayer += tp.OnUIClick;
        containedPlayers.Add(name);
        UpdateColors();

        //string name = (string)player.CustomProperties["name"];
        //GameObject newPlayer = Instantiate(tabPlayerPrefab, contentHolder);
        //TabPlayer tp = newPlayer.GetComponent<TabPlayer>();
        //tp.SetName(name);
        //tp.SetNameColor(alivePlayerColor);
        //OnClickPlayer += tp.OnUIClick;
        //tp.uPlayerList = this;
        //tp.player = player;
        //containedPlayers.Add(name);
        //UpdateColors();
    }

    void UpdateColors()
    {
        foreach (Transform child in  contentHolder.transform)
        {
            TabPlayer tp = child.GetComponent<TabPlayer>();
            if (child.GetSiblingIndex() % 2 == 0)
            {
                tp.SetPanelColor(primaryPanelColor);
            } else
            {
                tp.SetPanelColor(secondaryPanelColor);
            }
        }
    }
}
