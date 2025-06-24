using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevicePlacement : MonoBehaviour
{
    private PlayerDropManager dropManager;
    private Device device;

    private void OnInitialize(object[] data)
    {
        dropManager = ((GameObject)data[0]).GetComponent<PlayerDropManager>();
        string deviceName = (string)data[2];
        device = (Device)ObjectManager.i.itemSearch[deviceName];
    }

    private void OnPrimaryUse()
    {
        dropManager.DevicePlacePress(device);
    }

    private void OnPrimaryRelease()
    {
        dropManager.DevicePlaceRelease();
    }
}
