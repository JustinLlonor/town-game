using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyUI : MonoBehaviour
{
    public TextMeshProUGUI keyText;
    public Image key;

    public void SetKeyAlpha(float alpha)
    {
        key.color = new Color(key.color.r, key.color.g, key.color.b, alpha);
    }

    public void SetKeyColor(Color color)
    {
        Debug.Log("Color set to " + color);
        keyText.color = color;
    }

    public void SetKey(string key)
    {
        keyText.text = key;
    }
}
