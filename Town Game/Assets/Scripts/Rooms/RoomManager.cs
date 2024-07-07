using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class RoomManager : MonoBehaviour
{
    public string testRoom;
    public List<MapRoom> playerRooms = new List<MapRoom>();
    public List<MapRoom> workRooms = new List<MapRoom>();

    private void Awake()
    {
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) AddWorker(PhotonNetwork.LocalPlayer, testRoom);
    }

    [PunRPC]
    public void AddWorker(Photon.Realtime.Player player, string roomName)
    {
        int index = Array.FindIndex(workRooms.ToArray(), room => room.name == roomName);
        if (index == -1) return;

        if (!workRooms[index].workers.Contains(player))
        {
            workRooms[index].workers.Add(player);
            return;
        }
        Debug.LogError("Player already works at specified location!");
    }

    [PunRPC]
    public void RemoveWorker(Photon.Realtime.Player player, string roomName)
    {
        int index = Array.FindIndex(workRooms.ToArray(), room => room.name == roomName);
        if (index == -1) return;

        if (workRooms[index].workers.Contains(player))
        {
            workRooms[index].workers.Remove(player);
            return;
        }
    }
}
