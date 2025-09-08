using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntAction
{
    public string actionName;
    [Tooltip("If this action is client-sided or not")]
    public bool isClient = false;
    /// <summary>
    /// The default value of enabled for network interactables. Can be changed directly for client interactables
    /// </summary>
    [Tooltip("If the action is enabled by default")]
    public bool enabled = true;
    public Color color = Color.white;
    public Color fillColor = new Color(0, 0, 0, 0.5607843f);
    public Color keyColor = Color.black;
    /// <summary>
    /// Called when the player interacts with this action. Called on both the server and client for server actions
    /// </summary>
    public ActionEvent onInteract;
    /// <summary>
    /// The index relating to the information of this interactable action. -1 means this is client-sided
    /// </summary>
    [HideInInspector] public int actionInfoIndex;
    [Header("Server interactable settings")]
    /// <summary>
    /// The default length for network interactables. All client actions will have a length of 0.
    /// </summary>
    public float length = 0f;
    public bool usePlayerLimiters;
    public bool useTimeModify;
    public bool useFilters = false;
    public ItemFilter[] filters;
    [Tooltip("AND means all the filters must be true in order for the action to be revealed. OR means that at least one filter must be true for the action to be revealed")]
    public FilterLogic filterLogic;
    
    public enum FilterLogic
    {
        Or = 0,
        And = 1
    }

    public delegate void ActionEvent(Player player);
}
