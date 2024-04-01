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
    public GameObject gameplayUI;

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
}
