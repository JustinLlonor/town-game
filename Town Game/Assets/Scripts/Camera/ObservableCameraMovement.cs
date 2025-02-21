using UnityEngine;

public class ObservableCameraMovement : CameraBehaviourBase
{
    bool firstFrame = true;
    public override void CameraLook(CameraMovement cam, CameraManager cameraManager, RunnerManager runnerManager)
    {
        Observable currentObservable = cameraManager.GetCurrentObservable();
        if (currentObservable == null) return;
        if (!(currentObservable is ItemObservable)) return;
        if (cam.primaryDown == 0f)
        {
            ((ItemObservable)currentObservable).ResetInteractable();
            firstFrame = true;
            return;
        }
        Ray ray = cameraManager.mainCamera.ScreenPointToRay(cam.GetMousePosition());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3f, cam.subInteractableMask))
        {
            ((ItemObservable)currentObservable).ReceiveInteractable(hit.transform.gameObject, firstFrame);
        } 
        else
        {
            ((ItemObservable)currentObservable).ResetInteractable();
        }
        firstFrame = false;
    }
}
