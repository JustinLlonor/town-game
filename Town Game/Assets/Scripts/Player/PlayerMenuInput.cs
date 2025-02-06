using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenuInput : MonoBehaviour
{
    private void OnPlayerMenu()
    {
        UIManager um = FindFirstObjectByType<UIManager>();
        if (um == null) return;
        um.OpenPlayerMenu();
    }
}
