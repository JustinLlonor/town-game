using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

// For disabling and enabling input
public class InputUIManager : MonoBehaviour
{
    public bool disableOnUI = true;
    PlayerInput playerInput;
    CameraManager cameraManager;
    NetworkObject no;

    private void Awake()
    {
        no = GetComponentInParent<NetworkObject>();
        if (no != null)
        {
            if (!no.HasInputAuthority) Destroy(this);
        }
        playerInput = gameObject.GetComponent<PlayerInput>();
        cameraManager = FindFirstObjectByType<CameraManager>();
        cameraManager.OnSwitchCameraMode += OnCameraChangedMode;
    }

    private void Start()
    {
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
