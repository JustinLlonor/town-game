using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static PlayerRoom;

public class PlayerRoom : NetworkBehaviour
{
    public MapRoom currentRoom;
    public LayerMask roomMask;
    public EnterRoom OnEnterRoom;
    public ExitRoom OnExitRoom;
    PlayerRef player;

    public delegate void EnterRoom(MapRoom room);
    public delegate void ExitRoom(MapRoom room);

    public override void Spawned()
    {
        player = gameObject.GetComponent<Player>().owner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != Mathf.Log(roomMask.value, 2)) return;
        MapRoom enteredRoom = other.gameObject.GetComponent<MapRoom>();
        if (enteredRoom == currentRoom) return;
        if (enteredRoom == null)
        {
            Debug.LogError("Room collider doesn't have room component!");
            return;
        }
        currentRoom = enteredRoom;
        OnEnterRoom?.Invoke(enteredRoom);
        if (Runner.IsServer) enteredRoom.onPlayerEnter?.Invoke(player);
        Debug.Log("Entered room: " + enteredRoom.roomName);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != Mathf.Log(roomMask.value, 2)) return;
        if (currentRoom == null) return;
        MapRoom exitedRoom = other.gameObject.GetComponent<MapRoom>();
        currentRoom = null;
        OnExitRoom?.Invoke(exitedRoom);
        if (Runner.IsServer) exitedRoom.onPlayerExit?.Invoke(player);
        Debug.Log("Exited room: " + exitedRoom.roomName);
    }
}
