using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeviceButtonUI : MonoBehaviour
{
    public RawImage icon;

    public void SetIcon(Texture2D texture)
    {
        icon.texture = texture;
    }
}
