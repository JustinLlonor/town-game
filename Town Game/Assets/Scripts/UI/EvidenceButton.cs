using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EvidenceButton : MonoBehaviour
{
    public CorpseUI cUI;
    public string text;

    public void ShowText()
    {
        cUI.descTxt.text = text;
        cUI.descObj.SetActive(true);
        cUI.nfObj.SetActive(false);
    }
}
