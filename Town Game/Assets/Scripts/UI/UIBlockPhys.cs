using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UIBlockPhys : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI roomText;
    public TextMeshProUGUI timeText;
    public Image blockImage;
    public RawImage stripeImage;
    public TextMeshProUGUI overlapText;
    public TextMeshProUGUI keyText;
    public GameObject keyImage;
    public Animator animator;
    public Image border;

    public void SetNameText(string name)
    {
        nameText.text = name;
    }

    public void SetRoomText(string room)
    {
        roomText.text = room;
    }

    public void SetTimeText(string time)
    {
        timeText.text = time;
    }

    public void SetBlockColor(Color color)
    {
        blockImage.color = color;
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        Color stripColor = Color.HSVToRGB(h, s, Mathf.Clamp01(v - 0.45f));
        stripeImage.color = stripColor;
        Color borderColor = Color.HSVToRGB(h, s, Mathf.Clamp01(v - 0.35f));
        border.color = borderColor;
    }

    public void SetOverlap(string overlap)
    {
        overlapText.text = overlap;
    }

    public void SetKeyVisibility(bool enabled)
    {
        keyImage.SetActive(enabled);
        overlapText.gameObject.SetActive(enabled);
    }

    public void SetKeyText(string key)
    {
        keyText.text = key;
    }

    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }
}
