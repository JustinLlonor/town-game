using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class DeviceInput : NetworkBehaviour
{
    public PhysDevice connectedDevice;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendInt(int value, RpcInfo info = default)
    {
        connectedDevice.ReceivedInput(value, info.Source);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendVector2(Vector2 value, RpcInfo info = default)
    {
        connectedDevice.ReceivedInput(value, info.Source);
    }
}
