using UnityEngine;

public class ObservableCameraMovement : CameraBehaviourBase
{
    public override void CameraLook(CameraMovement cam, CameraManager cameraManager, RunnerManager runnerManager)
    {
        Ray ray = Camera.main.ScreenPointToRay(cam.GetMousePosition());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3f, cam.subInteractableMask))
        {
            
        }
    }
}
