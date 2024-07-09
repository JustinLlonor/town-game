using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapRoom : MonoBehaviour
{
    public string roomName;
    public RoomType roomType = RoomType.Living;
    public Transform spawnTransform;
    public Transform viewTransform; // The transform of the camera when the player selects this building
    public TaskHolder taskHolder;
    public List<Photon.Realtime.Player> workers = new List<Photon.Realtime.Player>();

    public enum RoomType
    {
        Work = 0,
        Living = 1,
        Misc = 2
    }
}
