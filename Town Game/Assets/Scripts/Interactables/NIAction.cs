using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NIAction
{
    public string actionName;
    [Tooltip("If the action is enabled by default")]
    public bool defaultEnabled = true;
    public float defaultLength = 0f;
    public Color color = Color.white;
    public Color fillColor = new Color(0, 0, 0, 0.5607843f);
    public Color keyColor = Color.black;
    /// <summary>
    /// Called when the player interacts with this action
    /// </summary>
    public ActionEvent onInteract;
    public int actionIndex;

    public delegate void ActionEvent(Player player);
}
