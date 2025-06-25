using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevicePlacement : MonoBehaviour
{
    private PlayerDropManager dropManager;
    private Device device;

    private void Initialize(ItemInitInfo info)
    {
        Debug.Log("Initialize called");
        dropManager = info.player.GetComponent<PlayerDropManager>();
        string deviceName = info.item;
        device = (Device)ObjectManager.i.itemSearch[deviceName];
    }

    private void OnPrimaryUse()
    {
        dropManager.DevicePlacePress(device);
    }

    private void OnPrimaryRelease()
    {
        dropManager.OnDropRelease();
    }
}
