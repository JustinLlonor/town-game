using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public string[] currentMaps = new string[] { };
    public string[] baseGameplayMaps = new string[] { };
    public string[] observableMaps = new string[] { };
    public string[] uiMaps = new string[] { };
    private bool uiOpen = false;
    [HideInInspector] public PlayerInput input;

    public delegate void InputEvent();
    public delegate void InputValueEvent(InputValue iv);

    private void Awake()
    {
        SetCurrentToBase();
        FindFirstObjectByType<CameraManager>().OnSwitchCameraMode += OnCameraChangeMode;
    }

    private void Start()
    {
        UIManager.instance.OnUIOpen += OnUIOpen;
        UIManager.instance.OnUIClose += OnUIClose;
    }

    // Map switching logic
    private void OnUIOpen()
    {
        uiOpen = true;
        DisableMaps(currentMaps);
        EnableMaps(uiMaps);
    }

    private void OnUIClose()
    {
        uiOpen = false;
        DisableMaps(uiMaps);
        EnableMaps(currentMaps);
    }

    private void SetCurrentToBase()
    {
        DisableMaps(currentMaps);
        currentMaps = baseGameplayMaps;
        if (!uiOpen) EnableMaps(currentMaps);
    }

    private void SetCurrentToObservable()
    {
        DisableMaps(currentMaps);
        currentMaps = observableMaps;
        if (!uiOpen) EnableMaps(currentMaps);
    }

    private void ClearAndDisableCurrent()
    {
        DisableMaps(currentMaps);
        currentMaps = new string[0];
    }

    private void OnCameraChangeMode(CameraManager.CameraMode mode)
    {
        switch (mode)
        {
            case CameraManager.CameraMode.FirstPerson:
                SetCurrentToBase();
                break;
            case CameraManager.CameraMode.Observe:
                SetCurrentToObservable();
                break;
            default:
                ClearAndDisableCurrent();
                break;
        }
    }

    private void DisableMaps(string[] maps)
    {
        foreach (string map in maps)
        {
            input.actions.FindActionMap(map).Disable();
        }
    }

    private void EnableMaps(string[] maps)
    {
        foreach (string map in maps)
        {
            input.actions.FindActionMap(map).Enable();
        }
    }

    // Inputs
    // BaseGameplay
    public InputEvent onJump;
    public InputValueEvent onMove;
    public InputValueEvent onSprint;
    public InputValueEvent onCrouch;
    public InputValueEvent onDropItem;
    public InputValueEvent onEquipItem;
    public InputEvent onPrimaryFire;
    public InputEvent onSecondaryFire;
    public InputValueEvent onCamera;
    // Interaction
    public InputValueEvent onInteract1;
    public InputValueEvent onInteract2;
    public InputValueEvent onInteract3;
    // Observable
    public InputEvent onPrimaryObserve;
    // UI
    public InputEvent onExit;
    // ClosedUI
    public InputEvent onPlayerMenu;

    private void OnJump() { onJump?.Invoke(); }
    private void OnMove(InputValue iv) { onMove?.Invoke(iv); }
    private void OnSprint(InputValue iv) { onSprint?.Invoke(iv); }
    private void OnCrouch(InputValue iv) { onCrouch?.Invoke(iv); }
    private void OnDropItem(InputValue iv) { onDropItem?.Invoke(iv); }
    private void OnEquipItem(InputValue iv) { onEquipItem?.Invoke(iv); }
    private void OnPrimaryFire() { onPrimaryFire?.Invoke(); }
    private void OnSecondaryFire() { onSecondaryFire?.Invoke(); }
    private void OnCamera(InputValue iv) { onCamera?.Invoke(iv); }
    private void OnInteract1(InputValue iv) { onInteract1?.Invoke(iv); }
    private void OnInteract2(InputValue iv) { onInteract2?.Invoke(iv); }
    private void OnInteract3(InputValue iv) { onInteract3?.Invoke(iv); }
    private void OnPrimaryObserve() { onPrimaryObserve?.Invoke(); }
    private void OnExit() { onExit?.Invoke(); }
    private void OnPlayerMenu() { onPlayerMenu?.Invoke(); }

}
