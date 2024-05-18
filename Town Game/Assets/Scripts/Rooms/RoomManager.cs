using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public List<MapRoom> playerRooms = new List<MapRoom>();

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<MapRoom>().isLivingQuarters)
            {
                playerRooms.Add(child.GetComponent<MapRoom>());
            }
        }
    }
}
