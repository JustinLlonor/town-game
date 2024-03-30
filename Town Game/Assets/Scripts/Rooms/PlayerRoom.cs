using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerRoom : MonoBehaviour
{
    public Room currentRoom;
    public LayerMask roomMask;
    public EnterRoom OnEnterRoom;
    public ExitRoom OnExitRoom;

    public delegate void EnterRoom(Room room);
    public delegate void ExitRoom(Room room);

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
        Room enteredRoom = other.gameObject.GetComponent<Room>();
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
        Room exitedRoom = other.gameObject.GetComponent<Room>();
        currentRoom = null;
        OnExitRoom?.Invoke(exitedRoom);
        Debug.Log("Exited room: " + exitedRoom.roomName);
    }

}
