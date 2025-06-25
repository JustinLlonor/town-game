using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using NUnit.Framework.Internal;

/// <summary>
/// Defines a volume in which a device can be placed in, stores every device within the volume
/// </summary>
public class DeviceVolume : NetworkBehaviour
{
    public Collider volumeCollider;
    /// <summary>
    /// All the devices that this DeviceVolume has. 
    /// A player can only place a device in a DeviceVolume if they are connected to the corresponding ControlPanel
    /// </summary>
    [Networked, Capacity(20)] public NetworkLinkedList<NetworkId> connectedDevices => default;
    public List<PlayerRef> containedPlayers = new List<PlayerRef>();
    public PlayerEvent onPlayerEnterVolume;
    public PlayerEvent onPlayerLeaveVolume;
    public DeviceVolumeEvent onConnectedDevicesUpdate;

    public delegate void PlayerEvent(PlayerRef player);
    public delegate void DeviceVolumeEvent();

    private ChangeDetector changeDetector;

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(connectedDevices):
                    onConnectedDevicesUpdate?.Invoke();
                    break;
            }
        }
    }

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

    public void AddDevice(NetworkId id)
    {
        connectedDevices.Add(id);
    }

    public void RemoveDevice(NetworkId id)
    {
        if (connectedDevices.Contains(id)) connectedDevices.Remove(id);
    }
}
