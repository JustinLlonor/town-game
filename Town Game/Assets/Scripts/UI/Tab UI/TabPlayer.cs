using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Linq;

public class TabPlayer : MonoBehaviour
{
    public string nick;
    public TextMeshProUGUI nameText;
    public RawImage perceptionHighlight;
    public RawImage positionIcon;
    public RawImage panel;
    public float iconX = 80.7f;
    public bool selected = false;
    [HideInInspector] public Photon.Realtime.Player player = null;
    [HideInInspector] public UIPlayerList uPlayerList;
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

    private void OnEnable()
    {
        selected = false;
        if (uPlayerList != null) uPlayerList.OnClickPlayer += OnUIClick;
    }

    private void OnDisable()
    {
        uPlayerList.OnClickPlayer -= OnUIClick;
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

    public void OnUIClick(Photon.Realtime.Player sPlayer = null)
    {
        if (sPlayer != player)
        {
            selected = false;
            return;
        }
        if (selected)
        {
            selected = false;
            uPlayerList.OnDeselectPlayer?.Invoke(player);
            return;
        }
        selected = true;
    }

    public void PlayerClick()
    {
        if (!PhotonNetwork.PlayerList.Contains(player)) return;
        uPlayerList.OnClickPlayer?.Invoke(player);
    }
}
