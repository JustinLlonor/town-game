using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PhysDevice : NetworkBehaviour
{
    public Texture2D icon;
    [Tooltip("The ui object this device is associated with")]
    public GameObject uiObject;
    [Tooltip("If this device takes input or not")]
    public bool takesInput = false;
    [Networked, Capacity(15)] public NetworkDictionary<PlayerRef, NetworkId> playerInputs => default;
    public NetworkPrefabRef deviceInputPrefab;
    [Tooltip("If this property is not null, then the game automatically adds this device to the specified volume")]
    //public DeviceVolume defaultVolume;

    public override void Spawned()
    {
        //if (defaultVolume != null) defaultVolume.AddDevice(Object);
    }

    /// <summary>
    /// Adds the player input object for this player
    /// </summary>
    /// <param name="player"></param>
    public void AddPlayerInput(PlayerRef player)
    {
        Debug.Log("Adding player input");
        if (!takesInput) return;
        if (playerInputs.ContainsKey(player)) return;
        Debug.Log("Instantiating object");
        NetworkObject newObject = Runner.Spawn(deviceInputPrefab, null, null, player);
        playerInputs.Add(player, newObject);
        newObject.GetComponent<DeviceInput>().connectedDevice = this;
    }

    /// <summary>
    /// Removes the player input object for this player
    /// </summary>
    /// <param name="player"></param>
    public void RemovePlayerInput(PlayerRef player)
    {
        if (!takesInput) return;
        if (!playerInputs.ContainsKey(player)) return;
        NetworkObject foundObject = null;
        Runner.TryFindObject(playerInputs[player], out foundObject);
        playerInputs.Remove(player);
        if (foundObject != null)
        {
            Runner.Despawn(foundObject);
        }
    }

    // Server sided events

    /// <summary>
    /// (Server side) Called when this device receives some input from the player.
    /// Use the "is" operator to determine the input type. Ex. input is string
    /// </summary>
    /// <param name="input"></param>
    /// <param name="player"></param>
    public virtual void ReceivedInput(object input, PlayerRef player) { }

    // Client sided events
    /// <summary>
    /// Called when the device is destroyed
    /// </summary>
    public virtual void DeviceDestroyed() { }

    /// <summary>
    /// Called when the player opens the device UI
    /// </summary>
    /// <param name="uiBehaviour">The ui behaviour this device is attached to, to be casted into the actual corresponding behaviour</param>
    public virtual void DeviceOpened(GameObject uiObject) { }

    /// <summary>
    /// Called when the player closes the device UI
    /// </summary>
    /// <param name="uiBehaviour"></param>
    public virtual void DeviceClosed() { }
}
