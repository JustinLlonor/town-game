using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerRoomChoose : NetworkBehaviour
{
    GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    /// <summary>
    /// Relays the chosen building to the server
    /// </summary>
    /// <param name="buildingName"></param>
    /// <param name="info"></param>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_ChooseBuilding(string buildingName, RpcInfo info = default)
    {
        gameManager.SetChosenBuilding(buildingName, info.Source);
    }
}
