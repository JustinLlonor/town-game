using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public List<Room> playerRooms = new List<Room>();

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Room>().isLivingQuarters)
            {
                playerRooms.Add(child.GetComponent<Room>());
            }
        }
    }
}
