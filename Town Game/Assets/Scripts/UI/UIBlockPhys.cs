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
        v = Mathf.Clamp01(v - 0.45f);
        Color newColor = Color.HSVToRGB(h, s, v);
        stripeImage.color = newColor;
    }
}
