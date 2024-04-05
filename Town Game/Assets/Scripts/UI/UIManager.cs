using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    CursorManager cm;
    InteractableFinder iFinder;

    public delegate void OpenUI();
    public delegate void CloseUI();

    private void Awake()
    {
        instance = this;
        cm = FindObjectOfType<CursorManager>();
        iFinder = FindObjectOfType<InteractableFinder>();
        OnUIClose += CloseCorpse;
        OnUIClose += SetOpenFalse;
        OnUIOpen += SetOpenTrue;
        PlayerManager pm = FindObjectOfType<PlayerManager>();
        FindObjectOfType<CameraManager>().OnSwitchCameraMode += OnCameraChangeMode;
    }

    private void OnExit()
    {
        OnUIClose.Invoke();
        cm.Lock();
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
}
