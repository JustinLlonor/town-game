using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public bool uiOpened = false;
    public static UIManager instance;
    public CorpseUI cUI;
    public UIMenuEvent OnUIOpen;
    public MenuEvent OnUIClose;
    public StatsUI statsUI;
    public GameObject gameplayUI;
    public GameObject hotbarUI;
    public GameObject glitchObject;
    public AttackQTE attackQTE;
    public UIPlayerList uip;
    public PositionUI pui;
    public MapMenuUI mapMenuUI;
    [Header("Menus")]
    public GameObject[] uiMenus;
    public int menuOpened = -1;

    CursorManager cm;
    InteractableFinder iFinder;
    InputManager inputManager;

    // Player Menu = 0, Map menu = 1, Inventory menu = 2, Settings = 3
    public delegate void UIMenuEvent(int menuIndex);
    public delegate void MenuEvent();

    private void Awake()
    {
        instance = this;
        cm = FindFirstObjectByType<CursorManager>();
        iFinder = FindFirstObjectByType<InteractableFinder>();
        foreach (var menu in uiMenus) menu.SetActive(true);
        OnUIClose += CloseCorpse;
        OnUIClose += CloseUI;
        OnUIClose += SetOpenFalse;
        OnUIOpen += UIOpen;
        PlayerManager pm = FindFirstObjectByType<PlayerManager>();
        FindFirstObjectByType<CameraManager>().onSwitchCameraMode += OnCameraChangeMode;
        inputManager = FindFirstObjectByType<InputManager>();
        //inputManager.onExit += ExitUIPressed;
        inputManager.onPlayerMenu += OpenTabMenu;
        inputManager.onMapMenu += OpenMapMenu;
        inputManager.onInventoryMenu += OpenInventoryMenu;
        uip.Init();
        if (pui != null) pui.Init();
        foreach (var menu in uiMenus) menu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(uiOpened);
            Debug.Log(menuOpened);
        }
    }

    private void UIOpen(int menu)
    {
        if (menuOpened == menu) return;
        menuOpened = menu;
        cm.Unlock();
        Cursor.visible = true;
        uiOpened = true;
        for (int i = 0; i < uiMenus.Length; i++)
        {
            if (i == menu)
            {
                uiMenus[i].SetActive(true);
                continue;
            }
            uiMenus[i].SetActive(false);
        }
        // Hide Gameplay UI if the map menu is open
        if (menu == 1)
        {
            gameplayUI.SetActive(false);
        }else
        {
            gameplayUI.SetActive(true);
        }

    }

    /// <summary>
    /// Attempts to close out of UI.
    /// </summary>
    public void ExitUI()
    {
        if (!uiOpened) return;
        uiOpened = false;
        OnUIClose?.Invoke();
        menuOpened = -1;
        gameplayUI.SetActive(true);
    }

    public void OpenTabMenu()
    {
        OpenMenu(0);
    }

    public void OpenMapMenu()
    {
        OpenMenu(1);
    }

    public void OpenInventoryMenu()
    {
        OpenMenu(2);
    }

    private void OpenMenu(int menuIndex)
    {
        int ogMenu = menuOpened;
        ExitUI();
        if (ogMenu != menuIndex) OnUIOpen?.Invoke(menuIndex);
    }

    // Corpse code //
    public void OpenCorpse(List<Evidence> evidence, string nickname, bool isCultist, int depth = 0)
    {
        cUI.gameObject.SetActive(true);
        cUI.CreateEvidenceList(evidence, depth);
        cUI.SetName(nickname);
        cUI.SetAlignment(isCultist);
        OnUIOpen.Invoke(-1);
    }

    public void CloseCorpse()
    {
        cUI.gameObject.SetActive(false);
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

    private void CloseUI()
    {
        Cursor.visible = false;
        cm.Lock();
        foreach (var menu in uiMenus) menu.SetActive(false);
    }
}
