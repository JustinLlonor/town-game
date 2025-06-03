using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerRoom : NetworkBehaviour
{
    public MapRoom currentRoom;
    public List<DeviceVolume> deviceVolumes = new List<DeviceVolume>();
    public string deviceVolumeTag;
    public string roomTag;
    public EnterRoom OnEnterRoom;
    public ExitRoom OnExitRoom;
    PlayerRef player;

    public delegate void EnterRoom(MapRoom room);
    public delegate void ExitRoom(MapRoom room);

    public override void Spawned()
    {
        player = Object.InputAuthority;
    }

    private void OnTriggerEnter(Collider other)
    {
        RoomEnterCheck(other);
        DVEnterCheck(other);
    }

    private void OnTriggerExit(Collider other)
    {
        RoomExitCheck(other);
        DVExitCheck(other);
    }

    private void DVEnterCheck(Collider other)
    {
        if (!Runner.IsServer) return;
        if (other.gameObject.tag != deviceVolumeTag) return;
        other.GetComponent<DeviceVolume>().OnPlayerEnter(player);
    }

    private void DVExitCheck(Collider other)
    {
        if (!Runner.IsServer) return;
        if (other.gameObject.tag != deviceVolumeTag) return;
        other.GetComponent<DeviceVolume>().OnPlayerExit(player);
    }

    private void RoomEnterCheck(Collider other)
    {
        if (other.gameObject.tag != roomTag) return;
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

    private void RoomExitCheck(Collider other)
    {
        if (other.gameObject.tag != roomTag) return;
        if (currentRoom == null) return;
        MapRoom exitedRoom = other.gameObject.GetComponent<MapRoom>();
        currentRoom = null;
        OnExitRoom?.Invoke(exitedRoom);
        if (Runner.IsServer) exitedRoom.onPlayerExit?.Invoke(player);
        Debug.Log("Exited room: " + exitedRoom.roomName);
    }
}
