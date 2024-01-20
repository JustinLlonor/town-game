using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public CorpseUI cUI;
    public CloseUI closeUI;

    CursorManager cm;

    public delegate void CloseUI();

    private void Awake()
    {
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
