using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Defines a volume in which a device can be placed in, stores every device within the volume
/// </summary>
public class DeviceVolume : NetworkBehaviour
{
    /// <summary>
    /// All the devices that this DeviceVolume has. 
    /// A player can only place a device in a DeviceVolume if they are connected to the corresponding ControlPanel
    /// </summary>
    [Networked, Capacity(20)] public NetworkLinkedList<NetworkId> connectedDevices => default;
    public List<PlayerRef> containedPlayers = new List<PlayerRef>();
    public PlayerEvent onPlayerEnterVolume;
    public PlayerEvent onPlayerLeaveVolume;

    public delegate void PlayerEvent(PlayerRef player);

    public bool PlayerContainedWithinVolume(PlayerRef player)
    {
        return containedPlayers.Contains(player);
    }

    public void OnPlayerEnter(PlayerRef player)
    {
        if (!containedPlayers.Contains(player)) containedPlayers.Add(player);
        onPlayerEnterVolume?.Invoke(player);
    }

    public void OnPlayerExit(PlayerRef player)
    {
        if (containedPlayers.Contains(player)) containedPlayers.Remove(player);
        onPlayerLeaveVolume?.Invoke(player);
    }
}
