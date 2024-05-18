using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerRoom : MonoBehaviour
{
    public MapRoom currentRoom;
    public LayerMask roomMask;
    public EnterRoom OnEnterRoom;
    public ExitRoom OnExitRoom;

    public delegate void EnterRoom(MapRoom room);
    public delegate void ExitRoom(MapRoom room);

    private void Awake()
    {
        if (!gameObject.GetComponent<PhotonView>().IsMine)
        {
            Destroy(this);
        }
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
        Debug.Log("Entered room: " + enteredRoom.roomName);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != Mathf.Log(roomMask.value, 2)) return;
        if (currentRoom == null) return;
        MapRoom exitedRoom = other.gameObject.GetComponent<MapRoom>();
        currentRoom = null;
        OnExitRoom?.Invoke(exitedRoom);
        Debug.Log("Exited room: " + exitedRoom.roomName);
    }

}
