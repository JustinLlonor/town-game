using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyUI : MonoBehaviour
{
    public TextMeshProUGUI keyText;
    public Image key;
    public RawImage rawImage;
    public CustomKey[] customKeys;

    [System.Serializable]
    public struct CustomKey
    {
        public string name;
        public Sprite customSpr;
    }

    public void SetKeyAlpha(float alpha)
    {
        key.color = new Color(key.color.r, key.color.g, key.color.b, alpha);
    }

    public void SetKeyColor(Color color)
    {
        keyText.color = color;
    }

    public void SetKey(string keyName)
    {
        foreach (CustomKey cKey in customKeys)
        {
            if (cKey.name == keyName)
            {
                keyText.enabled = false;
                key.sprite = cKey.customSpr;
                return;
            }
        }
        keyText.enabled = true;
        keyText.text = keyName;
    }
}
