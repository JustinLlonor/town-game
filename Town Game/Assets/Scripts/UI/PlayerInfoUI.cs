using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    public TextMeshProUGUI roleText;
    public Transform lineHolder;
    public RawImage[] folderImages = new RawImage[] { };
    public Color cultistColor;
    public Color researcherColor;

    private void Awake()
    {
        FindObjectOfType<RoleRevealer>().OnGetRole += SetRole;
    }

    void SetRole(bool isCultist)
    {
        Color setColor;
        if (isCultist)
        {
            setColor = cultistColor;
            roleText.text = "Cultist";
        } 
        else
        {
            roleText.text = "Researcher";
            setColor = researcherColor;
        }

        foreach (RawImage img in folderImages) img.color = setColor;
    }

    /// <summary>
    /// Sets the text on a line
    /// </summary>
    /// <param name="line"></param>
    /// <param name="text"></param>
    public void SetLineText(int line, string text)
    {
        TextMeshProUGUI lineText = lineHolder.GetChild(line).GetComponentInChildren<TextMeshProUGUI>();
        lineText.text = text;
    }
}
