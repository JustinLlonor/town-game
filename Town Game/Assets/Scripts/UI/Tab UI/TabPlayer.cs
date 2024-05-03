using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabPlayer : MonoBehaviour
{
    public string nick;
    public TextMeshProUGUI nameText;
    public RawImage perceptionHighlight;
    public RawImage positionIcon;
    public RawImage panel;
    public float iconX = 80.7f;
    float ogX;

    public enum Perception
    {
        None,
        Friend,
        Suspect,
        Cultist,
        Missing
    }

    public enum Position
    {
        Researcher,
        Guard,
        Leader
    }

    private void Awake()
    {
        ogX = gameObject.GetComponent<RectTransform>().anchoredPosition.x;
    }

    public void SetName(string name)
    {
        nick = name;
        nameText.text = name;
    }

    public void SetNameColor(Color color)
    {
        nameText.color = color;
    }

    public void CrossName(bool isCrossed)
    {
        if (isCrossed)
        {
            nameText.fontStyle = FontStyles.Strikethrough;
            return;
        }
        nameText.fontStyle &= ~FontStyles.Strikethrough;
    }

    public void HidePerception()
    {
        perceptionHighlight.gameObject.SetActive(false);
    }

    public void SetPerceptionColor(Color color)
    {
        perceptionHighlight.gameObject.SetActive(true);
        perceptionHighlight.color = color;
    }

    public void SetPositionIcon(Texture2D icon = null)
    {
        positionIcon.texture = icon;
    }

    public void SetPanelColor(Color color)
    {
        panel.color = color;
    }
}
