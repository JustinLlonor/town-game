using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public CorpseUI cUI;
    public CloseUI closeUI;

    CursorManager cm;

    public delegate void CloseUI();

    private void Awake()
    {
        instance = this;
        cm = FindObjectOfType<CursorManager>();
        closeUI += CloseCorpse;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            closeUI.Invoke();
            cm.Lock();
        }
    }



    // Corpse code //
    public void OpenCorpse(List<Evidence> evidence, int depth = 0)
    {
        cUI.gameObject.SetActive(true);
        cUI.CreateEvidenceList(evidence, depth);
    }

    public void CloseCorpse()
    {
        cUI.gameObject.SetActive(false);
    }
}
