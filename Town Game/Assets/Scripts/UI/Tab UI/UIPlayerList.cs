using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class UIPlayerList : MonoBehaviourPunCallbacks
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

    // Updates player list off of players in room
    public void UpdatePlayerList()
    {
        List<GameObject> toDestroy = new List<GameObject>();
        // Update individual cards, destroy unnecessary
        foreach (Transform child in contentHolder.transform)
        {
            TabPlayer tp = child.GetComponent<TabPlayer>();

            // Disconnected behaviour
            bool disconnected = true;
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if ((string)player.CustomProperties["name"] == tp.nick)
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
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (!containedPlayers.Contains(player.CustomProperties["name"]))
            {
                AddPlayer((string)player.CustomProperties["name"]);
            }
        }

        // Have something which updates positions later

        UpdateColors();
    }

    public void AddPlayer(string name)
    {
        GameObject newPlayer = Instantiate(tabPlayerPrefab, contentHolder);
        TabPlayer tp = newPlayer.GetComponent<TabPlayer>();
        tp.SetName(name);
        tp.SetNameColor(alivePlayerColor);
        containedPlayers.Add(name);
        UpdateColors();
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

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerList();
    }
}
