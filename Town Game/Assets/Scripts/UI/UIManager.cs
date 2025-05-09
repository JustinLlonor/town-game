using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public bool uiOpened = false;
    public static UIManager instance;
    public CorpseUI cUI;
    public OpenUI OnUIOpen;
    public CloseUI OnUIClose;
    public StatsUI statsUI;
    public GameObject gameplayUI;
    public GameObject hotbarUI;
    public TabUI tabUI;
    public GameObject glitchObject;
    public AttackQTE attackQTE;
    public UIPlayerList uip;
    public PositionUI pui;

    CursorManager cm;
    InteractableFinder iFinder;

    public delegate void OpenUI();
    public delegate void CloseUI();

    private void Awake()
    {
        instance = this;
        cm = FindFirstObjectByType<CursorManager>();
        iFinder = FindFirstObjectByType<InteractableFinder>();
        tabUI.gameObject.SetActive(true);
        OnUIClose += CloseCorpse;
        OnUIClose += CloseTabMenu;
        OnUIClose += SetOpenFalse;
        OnUIOpen += SetOpenTrue;
        OnUIOpen += cm.Unlock;
        PlayerManager pm = FindFirstObjectByType<PlayerManager>();
        FindFirstObjectByType<CameraManager>().onSwitchCameraMode += OnCameraChangeMode;
        InputManager inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onExit += ExitUI;
        uip.Init();
        if (pui != null) pui.Init();
        tabUI.gameObject.SetActive(false);
    }

    /// <summary>
    /// Attempts to close out of UI.
    /// </summary>
    public void ExitUI()
    {
        if (!uiOpened) return;
        OnUIClose.Invoke();
        cm.Lock();
    }

    public void OpenPlayerMenu()
    {
        if (tabUI == null) return;
        OnUIOpen.Invoke();
        OpenTabMenu();
    }

    // Corpse code //
    public void OpenCorpse(List<Evidence> evidence, string nickname, bool isCultist, int depth = 0)
    {
        cUI.gameObject.SetActive(true);
        cUI.CreateEvidenceList(evidence, depth);
        cUI.SetName(nickname);
        cUI.SetAlignment(isCultist);
        OnUIOpen.Invoke();
    }

    public void CloseCorpse()
    {
        cUI.gameObject.SetActive(false);
    }

    void SetOpenTrue()
    {
        uiOpened = true;
    }

    void SetOpenFalse()
    {
        uiOpened = false;
    }

    void OnCameraChangeMode(CameraManager.CameraMode mode)
    {
        if (gameplayUI == null) return;

        bool enabled = mode == CameraManager.CameraMode.FirstPerson;
        gameplayUI.SetActive(enabled);
        hotbarUI.SetActive(enabled);
    }

    public void OpenTabMenu()
    {
        tabUI.gameObject.SetActive(true);
        OnUIOpen.Invoke();
        tabUI.UpdatePlayerList();
        Cursor.visible = true;
        //tabUI.playerList.OnDeselectPlayer?.Invoke(null);
    }

    public void CloseTabMenu()
    {
        Cursor.visible = false;
        tabUI.gameObject.SetActive(false);
    }
}
