using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapRoom : MonoBehaviour
{
    public string roomName;
    public RoomCategory roomCategory = RoomCategory.House;
    public Transform spawnTransform;
    public Transform viewTransform; // The transform of the camera when the player selects this building
}
