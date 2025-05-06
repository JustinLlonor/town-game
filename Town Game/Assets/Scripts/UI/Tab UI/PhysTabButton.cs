using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhysTabButton : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image buttonImage;

    public void SetTextColor(Color color)
    {
        text.color = color;
    }

    public void SetButtonColor(Color color)
    {
        buttonImage.color = color;
    }
}
