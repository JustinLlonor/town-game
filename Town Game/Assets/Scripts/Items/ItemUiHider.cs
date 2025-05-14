using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUiHider : MonoBehaviour
{
    public ItemUIInfo iuii;

    public void HideUI()
    {
        iuii.HideObject();
    }
}
