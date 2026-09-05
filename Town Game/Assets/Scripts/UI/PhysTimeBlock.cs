using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PhysTimeBlock : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    public void ChangeTimeText(string text)
    {
        timeText.text = text;
    }
}
