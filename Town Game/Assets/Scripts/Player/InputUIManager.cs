using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// For disabling and enabling input
public class InputUIManager : MonoBehaviour
{
    public bool disableOnUI = true;
    PlayerInput playerInput;
    CameraManager cameraManager;

    private void Awake()
    {
        playerInput = gameObject.GetComponent<PlayerInput>();
        cameraManager = FindObjectOfType<CameraManager>();
        cameraManager.OnSwitchCameraMode += OnCameraChangedMode;
    }

    private void Start()
    {
        //if (gameObject.GetComponent<PhotonView>() != null)
        //{
        //    if (!gameObject.GetComponent<PhotonView>().IsMine) return;
        //}
        UIManager.instance.OnUIOpen += DisableInputs;
        UIManager.instance.OnUIClose += EnableInputs;
    }

    public void EnableInputs()
    {
        if (playerInput != null) playerInput.enabled = disableOnUI;
    }

    public void DisableInputs()
    {
        if (playerInput != null) playerInput.enabled = !disableOnUI;
    }

    void OnCameraChangedMode(CameraManager.CameraMode mode)
    {
        if (mode == CameraManager.CameraMode.FirstPerson)
        {
            EnableInputs();
            return;
        }
        DisableInputs();
    }
}
