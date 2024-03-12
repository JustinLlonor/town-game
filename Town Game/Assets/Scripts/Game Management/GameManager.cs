using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using Unity.VisualScripting;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // gamePhase 0 = initialize game/assign roles 1 = main game 2 = results screen
    public int gamePhase = 0;
    [Header("Game Variables")]
    public Player campLeader;
    public Player[] cultists = new Player[] { };
    [Header("Game Settings")]
    // When an index of this is true, a cultist is added when the players playing is equal to that number.
    public bool[] cultistAssignment = new bool[] { };
    PhotonView view;

    // Open when need new variable to synchronize
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gamePhase);
            stream.SendNext(cultists);
        }
        else
        {
            gamePhase = (int)stream.ReceiveNext();
            cultists = (Player[])stream.ReceiveNext();
        }
    }
        
    private void Awake()
    {
        view = transform.GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhaseProperties();
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
        Debug.LogError(PhotonNetwork.LocalPlayer.CustomProperties["isCultist"]);
    }
}
