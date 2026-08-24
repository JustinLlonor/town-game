using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Subtask : ScriptableObject
{
    public enum CheckMode
    {
        None = 0,
        AtLeastOne = 1,
        AllPlayers
    }

    /// <summary>
    /// The display name of this subtask
    /// </summary>
    public string displayName;
    public Texture2D icon;
    [Tooltip("If enabled, the next subtask must always have this subtask completed for it to be active." +
        "If disabled, this subtask only needs to be completed once for the next subtask to activate")]
    public bool requireCompleted = false;
    [Tooltip("How subtasks will be checked for completion. " +
        "If set to none, the Player parameter in IsCompleted will be passed in as null. " +
        "If set to AtLeastOne, then at least one player assigned must fulfill the subtask requirements. " +
        "If set to AllPlayers, then all players assigned must fulfill subtask requirements.")]
    public CheckMode completionMode; // Can set a default value in child classes

    /// <summary>
    /// Called when the current active subtask is this (CLIENT SIDE)
    /// </summary>
    public abstract void OnActivateClient();
    /// <summary>
    /// Called when this subtask is no longer the current active subtask (CLIENT SIDE)
    /// </summary>
    public abstract void OnDeactivateClient();
    /// <summary>
    /// Checks if this subtask has been completed (SERVER SIDE)
    /// </summary>
    /// <param name="player">The player that we are checking</param>
    /// <returns>If this subtask has been completed or not</returns>
    public abstract bool IsCompleted(Player player = null);
}