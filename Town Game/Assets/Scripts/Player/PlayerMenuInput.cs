using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenuInput : MonoBehaviour
{
    private void OnPlayerMenu()
    {
        UIManager um = FindObjectOfType<UIManager>();
        if (um == null) return;
        um.OpenPlayerMenu();
    }
}
