using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerInfoUI : MonoBehaviour
{
    public TextMeshProUGUI roleText;
    public Color cultistColor;
    public Color researcherColor;
    GameManager gm;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        FindObjectOfType<RoleRevealer>().OnGetRole += SetRole;
        //gm.OnUpdatePositions += OnUpdatePositions;
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
            roleText.text = "Civilian";
            setColor = researcherColor;
        }

        roleText.color = setColor;
    }

    /// <summary>
    /// Sets the text on a line
    /// </summary>
    /// <param name="line"></param>
    /// <param name="text"></param>
    public void SetLineText(int line, string text)
    {
        //TextMeshProUGUI lineText = lineHolder.GetChild(line).GetComponentInChildren<TextMeshProUGUI>();
        //lineText.text = text;
    }

  //  void OnUpdatePositions()
  //  {
  //      string currentName = (string)PhotonNetwork.LocalPlayer.CustomProperties["name"];
  //      if (gm.playerPositions.ContainsKey(currentName))
  //      {
  //          int currentPosition = (int)gm.playerPositions[currentName];
  //          string newText = $"Position: {gm.positions[currentPosition]}";
  //          SetLineText(0, newText);
  //      }
  //  }
}
