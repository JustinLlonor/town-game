using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using Photon.Pun;
using Fusion;
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
    public PlayerRef player = PlayerRef.None;
    [HideInInspector] public UIPlayerList uPlayerList;
    public Transform voteHolder;
    public GameObject voteButton;
    RunnerManager rm;
    ObjectManager objectManager;
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
        rm = FindFirstObjectByType<RunnerManager>();
        ogX = gameObject.GetComponent<RectTransform>().anchoredPosition.x;
        objectManager = FindFirstObjectByType<ObjectManager>();
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

    public void OnUIClick(PlayerRef sPlayer)
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
        if (!rm.nRunner.ActivePlayers.Contains(player)) return;
        uPlayerList.OnClickPlayer?.Invoke(player);
    }

    // Called if this player can be voted for
    public void AddVoteButton(ClientVoteInstance vote, bool canVote, UnityEngine.Events.UnityAction call)
    {
        GameObject button = Instantiate(voteButton, voteHolder);
        int siblingIndex = GetIdIndex(vote.id);
        button.transform.SetSiblingIndex(siblingIndex);
        PhysVoteButton vb = button.GetComponent<PhysVoteButton>();
        vb.SetDescription(vote.voteAction.ToString());
        vb.SetVoteIcon(objectManager.texSearch[vote.iconId.ToString()]);
        vb.SetCanVote(canVote);
        vb.button.onClick.AddListener(call);
    }

    /// <summary>
    /// Destroys all vote buttons on this tab player with the specified id
    /// </summary>
    /// <param name="id"></param>
    public void RemoveVoteButton(int id)
    {
        foreach (Transform child in voteHolder)
        {
            PhysVoteButton vb = child.GetComponent<PhysVoteButton>();
            if (vb.id == id) Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Updates the vote count for all vote buttons in this player with the specified id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="voteCount"></param>
    public void UpdateVoteCount(int id, int voteCount)
    {
        foreach (Transform child in voteHolder)
        {
            PhysVoteButton vb = child.GetComponent<PhysVoteButton>();
            if (vb.id == id) vb.SetVoteCount(voteCount);
        }
    }

    /// <summary>
    /// Gets the sibling index for a vote button, given an id. Lower ids get a lower index
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private int GetIdIndex(int id)
    {
        // Finds the sibling index between the id that is greater and the id that is less than it
        foreach (Transform child in voteHolder)
        {
            PhysVoteButton vb = child.GetComponent<PhysVoteButton>();
            if (id < vb.id) return child.GetSiblingIndex();
        }
        return voteHolder.childCount;
    }
}
