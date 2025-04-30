using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhysVoteButton : MonoBehaviour
{
    public int id;
    public TextMeshProUGUI voteCount;
    public RawImage voteIcon;
    public DescriptionHover dHover;

    /// <summary>
    /// Sets the vote count text for the vote button
    /// </summary>
    /// <param name="newCount"></param>
    public void SetVoteCount(int newCount)
    {
        if (newCount == 0)
        {
            voteCount.text = "";
            return;
        }
        voteCount.text = newCount.ToString();
    }

    /// <summary>
    /// Sets the icon for the vote button
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="color"></param>
    public void SetVoteIcon(Texture texture, Color color)
    {
        voteIcon.texture = texture;
        voteIcon.color = color;
    }

    /// <summary>
    /// Sets the icon for the vote button, with the color white
    /// </summary>
    /// <param name="texture"></param>
    public void SetVoteIcon(Texture texture)
    {
        voteIcon.texture = texture;
        voteIcon.color = Color.white;
    }

    public void SetDescription(string description)
    {
        dHover.description = description;
    }
}
