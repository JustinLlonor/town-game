using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDevice : PhysDevice      
{
    public Camera attachedCamera;

    public override void DeviceOpened(GameObject uiObject)
    {
        base.DeviceOpened(uiObject);
        attachedCamera.enabled = true;
        attachedCamera.targetTexture = uiObject.GetComponent<CameraDeviceUI>().cameraTexture;
    }

    public override void DeviceClosed()
    {
        base.DeviceClosed();
        attachedCamera.enabled = false;
    }
}
