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
    PlayerManager playerManager;

    // TODO: Disconnect on death or player leave
    // TODO: Player interest should only show connected devices when a player can access it
    public override void Spawned()
    {
        base.Spawned();
        positionManager = FindAnyObjectByType<PositionManager>();
        playerManager = FindAnyObjectByType<PlayerManager>();
        connectedVolume.onPlayerLeaveVolume += Disconnect;
        interactable.onLook += CheckConnection;
        positionManager.onJobAdd += CheckConnection;
        positionManager.onJobRemove += CheckConnection;
    }

    private void ConnectDevices(PlayerRef player)
    {
        foreach (NetworkId nID in connectedVolume.connectedDevices)
        {
            PhysDevice device = GetPhysDevice(nID);
            if (device == null) continue;
            device.AddPlayerInput(player);
        }
    }

    private void DisconnectDevices(PlayerRef player)
    {
        foreach (NetworkId nID in connectedVolume.connectedDevices)
        {
            PhysDevice device = GetPhysDevice(nID);
            if (device == null) continue;
            device.RemovePlayerInput(player);
        }
    }

    private PhysDevice GetPhysDevice(NetworkId id)
    {
        NetworkObject no = null;
        Runner.TryFindObject(id, out no);
        if (no != null) return no.GetComponent<PhysDevice>();
        return null;
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
            CheckPlayerControlPanels(player);
            AddConnectedPlayer(player);
            RPC_SendConnected(player, true);
            ConnectDevices(player);
        }
    }

    public void Disconnect(PlayerRef player)
    {
        RemoveConnectedPlayer(player);
        DisconnectDevices(player);
        DeselectPlayerControlPanel(player);
        RPC_SendConnected(player, false);
    }

    private void DeselectPlayerControlPanel(PlayerRef player)
    {
        NetworkObject playerObject = playerManager.GetPlayerNetworkObject(player);
        if (playerObject == null) return;
        Player playerBehaviour = playerObject.GetComponent<Player>();
        playerBehaviour.connectedPanel = null;
    }

    /// <summary>
    /// Disconnects from any control panels the player may have connected to, and sets their connected panel to this
    /// </summary>
    /// <param name="player"></param>
    private void CheckPlayerControlPanels(PlayerRef player)
    {
        NetworkObject playerObject = playerManager.GetPlayerNetworkObject(player);
        if (playerObject != null)
        {
            Player playerBehaviour = playerObject.GetComponent<Player>();
            if (playerBehaviour.connectedPanel != null)
            {
                playerBehaviour.connectedPanel.Disconnect(player);
            }
            playerBehaviour.connectedPanel = this;
        }
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
    public void RPC_SendConnected([RpcTarget] PlayerRef player, bool isConnected)
    {
        Player playerB = null;
        if (playerManager.currentPlayer != null)
        {
            playerB = playerManager.currentPlayer.GetComponent<Player>();
        }
        if (isConnected)
        {
            SetDisconnect();
            ConnectDeviceUI();
            playerB.connectedClientPanel = this;
        }
        else
        {
            SetConnect();
            // For the case of switching control panels, if the disconnect packet arrives after the new connection, won't clear device UI
            if (playerB.connectedClientPanel == this)
            {
                ClearDeviceUI();
                playerB.connectedClientPanel = null;
            }
        }
        connected = isConnected;
    }

    private void ConnectDeviceUI()
    {
        MapMenuUI mmUI = UIManager.instance.mapMenuUI;
        mmUI.ClearDeviceButtons();
        foreach (NetworkId deviceId in connectedVolume.connectedDevices)
        {
            PhysDevice device = GetPhysDevice(deviceId);
            if (device == null) continue;
            mmUI.AddDeviceButton(device);
        }
    }

    private void ClearDeviceUI()
    {
        MapMenuUI mmUI = UIManager.instance.mapMenuUI;
        mmUI.ClearDeviceButtons();
    }

    public void SwapConnection()
    {
        interactable.hovers[0].lore = "";
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
