using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using UnityEditor.Animations;

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
    private List<ClientVoteInstance> uiVotes = new List<ClientVoteInstance>();

    public ClickPlayer OnClickPlayer;
    public DeselectPlayer OnDeselectPlayer;
    public delegate void ClickPlayer(PlayerRef player);
    public delegate void DeselectPlayer(PlayerRef player);

    PlayerManager playerManager;
    RunnerManager rm;
    VotingManager votingManager;
    GameManager gameManager;

    private void Awake()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        rm = FindFirstObjectByType<RunnerManager>();
        votingManager = FindFirstObjectByType<VotingManager>();
        rm.onPlayerJoin += PlayerEventUpdatePlayerList;
        rm.onPlayerLeave += PlayerEventUpdatePlayerList;
        votingManager.onReceiveVote += AddVoteToPlayers;
    }

    private void Update()
    {
        CheckVotes();
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
            // Iterate over every online palyer
            foreach (PlayerRef player in playerList)
            {
                if (playerManager.GetPlayerNetworkObject(player).GetComponent<Player>().nickname == tp.nick)
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
            if (!containedPlayers.Contains(playerManager.GetPlayerNetworkObject(player).GetComponent<Player>().nickname))
            {
                AddPlayer(player);
            }
        }

        // Have something which updates positions later

        UpdateColors();
    }

    public void AddPlayer(PlayerRef player)
    {
        string name = playerManager.GetPlayerNetworkObject(player).GetComponent<Player>().nickname;
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

    private void AddVoteToPlayers(ClientVoteInstance vote, NetworkBool canVote)
    {
        // Starts tracking the vote
        uiVotes.Add(vote);

        // Add the vote button to every player that is on the voted list
        List<PlayerRef> votedPlayers = new List<PlayerRef>(vote.votedWhitelist);
        foreach (Transform child in contentHolder.transform)
        {
            TabPlayer tabPlayer = child.GetComponent<TabPlayer>();
            if (votedPlayers.Contains(tabPlayer.player))
            {
                tabPlayer.AddVoteButton(vote, canVote);
            }
        }
    }

    /// <summary>
    /// Iterates over every vote. If one of them is expired, delete it
    /// </summary>
    private void CheckVotes()
    {
        foreach (ClientVoteInstance vote in uiVotes)
        {
            if (VoteExpired(vote))
            {
                RemoveVoteFromPlayers(vote.id);
            }
        }
    }

    /// <summary>
    /// Removes the vote instance of the specified id from every element
    /// </summary>
    /// <param name="id"></param>
    private void RemoveVoteFromPlayers(int id)
    {
        foreach (Transform child in contentHolder.transform)
        {
            TabPlayer tabPlayer = child.GetComponent<TabPlayer>();
            tabPlayer.RemoveVoteButton(id);
        }
    }

    /// <summary>
    /// Uses game time to check if the vote has expired
    /// </summary>
    /// <param name="vote"></param>
    /// <returns></returns>
    private bool VoteExpired(ClientVoteInstance vote)
    {
        return vote.voteTimeEnd > gameManager.gameTime;
    }
}
