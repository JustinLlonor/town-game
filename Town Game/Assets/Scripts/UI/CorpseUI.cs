using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CorpseUI : MonoBehaviour
{
    [Header("Evidence")]
    public Transform eContent;
    public GameObject evidencePrefab;
    [Header("Description")]
    public TextMeshProUGUI descTxt;
    public GameObject descObj;
    public GameObject nfObj;

    CursorManager cm;
    ObjectManager om;

    private void Awake()
    {
        cm = FindObjectOfType<CursorManager>();
        om = FindObjectOfType<ObjectManager>();
    }

    public void CreateEvidenceList(List<Evidence> evidence, int depth)
    {
        ResetUI();
        cm.Unlock();
        foreach (Evidence e in evidence)
        {
            GameObject newEvidence = Instantiate(evidencePrefab, eContent);
            newEvidence.transform.GetChild(1).GetComponent<RawImage>().texture = om.texSearch[e.icons[depth]];
            EvidenceButton eButton = newEvidence.GetComponent<EvidenceButton>();
            eButton.cUI = this;
            eButton.text = e.descriptions[depth];
        }
    }

    void ResetUI()
    {
        nfObj.SetActive(true);
        descObj.SetActive(false);
        foreach (Transform child in eContent)
        {
            Destroy(child.gameObject);
        }
    }
}
