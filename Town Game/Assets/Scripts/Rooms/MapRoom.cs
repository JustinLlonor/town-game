using Fusion;
using JetBrains.Annotations;
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
    /// <summary>
    /// The amount of energy that is gained or lost when a player spends the night in this room
    /// </summary>
    public int energyDiff = -1;
    /// <summary>
    /// The delegate that is called when a player enters a room
    /// </summary>
    public PlayerEvent onPlayerEnter;
    /// <summary>
    /// The delegate that is called when a player exits a room
    /// </summary>
    public PlayerEvent onPlayerExit;
    /// <summary>
    /// Called when a player gains access to this MapRoom or loses access to the MapRoom
    /// </summary>
    public PlayerEvent onAccessUpdate; //TODO: Implementation

    public delegate void PlayerEvent(PlayerRef player);
}
