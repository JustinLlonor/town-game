using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PhysDevice : NetworkBehaviour
{
    public DeviceVolume volume;

    // Client sided events
    /// <summary>
    /// Called when the device is destroyed
    /// </summary>
    public virtual void DeviceDestroyed() { }

    /// <summary>
    /// Called when the player opens the device UI
    /// </summary>
    /// <param name="uiBehaviour">The ui behaviour this device is attached to, to be casted into the actual corresponding behaviour</param>
    public virtual void DeviceOpened(DeviceUI uiBehaviour) { }

    /// <summary>
    /// Called when the player closes the device UI
    /// </summary>
    /// <param name="uiBehaviour"></param>
    public virtual void DeviceClosed() { }
}
