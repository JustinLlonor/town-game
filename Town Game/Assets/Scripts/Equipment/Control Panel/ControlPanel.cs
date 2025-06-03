using Fusion;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a control panel used to control devices
/// </summary>
public class ControlPanel : Equipment
{
    public List<PlayerRef> connectedPlayers;
    public DeviceVolume connectedVolume;
    public Interactable interactable;
    // Client sided attribute if the player is connected or not
    private bool connected = false;
    PositionManager positionManager;

    public override void Spawned()
    {
        base.Spawned();
        positionManager = FindAnyObjectByType<PositionManager>();
        connectedVolume.onPlayerLeaveVolume += Disconnect;
        interactable.onLook += CheckConnection;
        positionManager.onJobAdd += CheckConnection;
        positionManager.onJobRemove += CheckConnection;
    }

    public void ToggleConnection(PlayerRef player)
    {
        if (!Runner.IsServer) return;
        if (!positionManager.PlayerHasAccessToRoom(player, room.roomName)) return;
        if (connectedPlayers.Contains(player))
        {
            Disconnect(player);
        }
        else
        {
            Connect(player);
        }
    }

    public void Connect(PlayerRef player)
    {
        if (connectedVolume.PlayerContainedWithinVolume(player))
        {
            AddConnectedPlayer(player);
            RPC_SendConnected(player, true);
        }
    }

    public void Disconnect(PlayerRef player)
    {
        RemoveConnectedPlayer(player);
        RPC_SendConnected(player, false);
    }

    private void AddConnectedPlayer(PlayerRef player)
    {
        if (connectedPlayers.Contains(player)) return;
        connectedPlayers.Add(player);
        Debug.Log("Connected player");
    }

    private void RemoveConnectedPlayer(PlayerRef player)
    {
        if (!connectedPlayers.Contains(player)) return;
        connectedPlayers.Remove(player);
        Debug.Log("Disconnected player");
    }

    // Client sided stuff

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendConnected([RpcTarget] PlayerRef player, bool connection)
    {
        if (connection)
        {
            SetDisconnect();
        }
        else
        {
            SetConnect();
        }
        connected = connection;
    }

    public void SwapConnection()
    {
        if (!positionManager.PlayerHasAccessToRoom(Runner.LocalPlayer, room.roomName)) return;
        connected = !connected;
        if (connected)
        {
            SetDisconnect();
        }
        else
        {
            SetConnect();
        }
    }

    public void SetCantConnect()
    {
        interactable.hovers[0].lore = "You don't have access to this control panel.";
        interactable.hovers[0].interactKey = Interactable.InteractKey.None;
        interactable.hovers[0].color = Color.gray;
    }

    public void SetConnect()
    {
        interactable.hovers[0].lore = "Connect";
        interactable.hovers[0].interactKey = Interactable.InteractKey.Interact1;
        interactable.hovers[0].color = Color.white;
    }

    public void SetDisconnect()
    {
        interactable.hovers[0].lore = "Disconnect";
        interactable.hovers[0].interactKey = Interactable.InteractKey.Interact1;
        interactable.hovers[0].color = Color.white;
    }

    /// <summary>
    /// Checks connection when a job  updates
    /// </summary>
    /// <param name="jRef"></param>
    public void CheckConnection(Vector2Int jRef)
    {
        if (!interactable.isLooking) return;
        CheckConnection();
    }

    public void CheckConnection()
    {
        if (!positionManager.PlayerHasAccessToRoom(Runner.LocalPlayer, room.roomName))
        {
            SetCantConnect();
            return;
        }
        if (connected)
        {
            SetDisconnect();
            return;
        }
        SetConnect();
    }
}
