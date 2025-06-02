using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a volume in which a device can be placed in
/// </summary>
public class DeviceVolume : MonoBehaviour
{
    // use an event for connectability when player enters/exits?
    public ControlPanel connectedPanel;
    List<Collider> collidedObjects;

    public delegate void DeviceVolumeEvent();

    private void OnTriggerEnter(Collider other)
    {
        collidedObjects.Add(other);
        PlayerEnterCheck(other);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerExitCheck(other);
        if (collidedObjects.Contains(other)) collidedObjects.Remove(other);
    }

    private void PlayerEnterCheck(Collider coll)
    {
        
    }

    private void PlayerExitCheck(Collider coll)
    {

    }
}
