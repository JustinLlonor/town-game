using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public string[] currentMaps = new string[] { };
    public string[] baseGameplayMaps = new string[] { };
    public string[] observableMaps = new string[] { };
    public MapHolder[] uiMenuMaps;
    public string[] buildingChooseMaps = new string[] { };
    private bool uiOpen = false;
    [HideInInspector] public PlayerInput input;
    private bool cinematicDisable = true;
    private string[] enabledUIMaps = new string[0];

    public delegate void InputEvent();
    public delegate void InputValueEvent(InputValue iv);

    [System.Serializable]
    public struct MapHolder
    {
        public string[] maps;
    }

    private void Awake()
    {
        SetCurrentToBase();
        FindFirstObjectByType<CameraManager>().onSwitchCameraMode += OnCameraChangeMode;
    }

    private void Start()
    {
        UIManager.instance.OnUIOpen += OnUIOpen;
        UIManager.instance.OnUIClose += OnUIClose;
    }

    // Map switching logic
    private void OnUIOpen(int menu)
    {
        uiOpen = true;
        string[] mapsEnabled = uiMenuMaps[menu].maps;
        if (enabledUIMaps.Length > 0) DisableMaps(enabledUIMaps);
        else DisableMaps(currentMaps);
        EnableMaps(mapsEnabled);
        enabledUIMaps = mapsEnabled;
    }

    private void OnUIClose()
    {
        enabledUIMaps = new string[0];
        uiOpen = false;
        DisableMaps(enabledUIMaps);
        EnableMaps(currentMaps);
    }

    public void SetCurrentToBase()
    {
        DisableMaps(currentMaps);
        currentMaps = baseGameplayMaps;
        if (!uiOpen) EnableMaps(currentMaps);
    }

    public void SetCurrentToObservable()
    {
        DisableMaps(currentMaps);
        currentMaps = observableMaps;
        if (!uiOpen) EnableMaps(currentMaps);
    }

    public void SetCurrentToBuildingChoose()
    {
        DisableMaps(currentMaps);
        currentMaps = buildingChooseMaps;
        cinematicDisable = false;
        if (uiOpen) OnExit();
        EnableMaps(currentMaps);
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
                if (!cinematicDisable)
                {
                    cinematicDisable = true;
                    break;
                }
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
    public InputEvent onScheduleSwap;
    public InputValueEvent onRotateMode;
    // Interaction
    public InputValueEvent onInteract1;
    public InputValueEvent onInteract2;
    public InputValueEvent onInteract3;
    // Observable
    public InputValueEvent onPrimaryObserve;
    public InputEvent onExitObserve;
    // UI
    public InputEvent onExit;
    // ClosedUI
    public InputEvent onPlayerMenu;
    public InputEvent onMapMenu;
    public InputEvent onInventoryMenu;
    public InputEvent onSettingsMenu;
    // Building
    public InputEvent onScrollLeft;
    public InputEvent onScrollRight;
    public InputEvent onChooseBuilding;
    // Voice
    public InputValueEvent onVoice;
    // MapUI
    public InputValueEvent onMapZoom;
    public InputValueEvent onMapDrag;

    private void OnJump() { onJump?.Invoke(); }
    private void OnMove(InputValue iv) { onMove?.Invoke(iv); }
    private void OnSprint(InputValue iv) { onSprint?.Invoke(iv); }
    private void OnCrouch(InputValue iv) { onCrouch?.Invoke(iv); }
    private void OnDropItem(InputValue iv) { onDropItem?.Invoke(iv); }
    private void OnEquipItem(InputValue iv) { onEquipItem?.Invoke(iv); }
    private void OnPrimaryFire() { onPrimaryFire?.Invoke(); }
    private void OnSecondaryFire() { onSecondaryFire?.Invoke(); }
    private void OnCamera(InputValue iv) { onCamera?.Invoke(iv); }
    private void OnScheduleSwap() { onScheduleSwap?.Invoke(); }
    private void OnRotateMode(InputValue iv) { onRotateMode?.Invoke(iv); }
    private void OnInteract1(InputValue iv) { onInteract1?.Invoke(iv); }
    private void OnInteract2(InputValue iv) { onInteract2?.Invoke(iv); }
    private void OnInteract3(InputValue iv) { onInteract3?.Invoke(iv); }
    private void OnPrimaryObserve(InputValue iv) { onPrimaryObserve?.Invoke(iv); }
    private void OnExit() { onExit?.Invoke(); }
    private void OnPlayerMenu() { onPlayerMenu?.Invoke(); }
    private void OnMapMenu() { onMapMenu?.Invoke(); }
    private void OnInventoryMenu() { onInventoryMenu?.Invoke(); }
    private void OnSettingsMenu() { onSettingsMenu?.Invoke(); }
    private void OnExitObserve() { onExitObserve?.Invoke(); }
    private void OnScrollLeft() { onScrollLeft?.Invoke(); }
    private void OnScrollRight() { onScrollRight?.Invoke(); }
    private void OnChooseBuilding() { onChooseBuilding?.Invoke(); }
    private void OnVoice(InputValue iv) { onVoice?.Invoke(iv); }
    private void OnMapZoom(InputValue iv) { onMapZoom?.Invoke(iv); }
    private void OnMapDrag(InputValue iv) { onMapDrag?.Invoke(iv);  }
}
