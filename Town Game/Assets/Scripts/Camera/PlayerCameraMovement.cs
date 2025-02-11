using Unity.VisualScripting;
using UnityEngine;

public class PlayerCameraMovement : CameraBehaviourBase
{
    public override void CameraLook(CameraMovement cam, CameraManager cameraManager, RunnerManager runnerManager)
    {
        if (cam.player == null) return;
        if (cameraManager.isTransitioning) return;
        if (!cam.canMove) return;
        if (!cam.CursorLocked()) return;
        Vector2 mouseMv = cam.GetMouseMovement();
        cam.xRotation -= mouseMv.y;
        cam.xRotation = Mathf.Clamp(cam.xRotation, -90, 90);
        cam.yRotation += mouseMv.x;
        cam.transform.eulerAngles = new Vector3(cam.xRotation, cam.yRotation, 0f);
        cam.orientation.eulerAngles = new Vector3(0, cam.yRotation, 0);
        cam.player.rotation = cam.orientation.rotation;
        cam.headAim.position = cam.transform.position + cam.transform.forward;
        runnerManager.orientation = cam.yRotation;
        runnerManager.camOrientation = cam.xRotation;
    }
}
