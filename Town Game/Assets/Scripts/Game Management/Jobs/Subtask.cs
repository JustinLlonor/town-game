using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Subtask : ScriptableObject
{
    /// <summary>
    /// The display name of this subtask
    /// </summary>
    public string displayName;
    public Texture2D icon;

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
    public abstract bool IsCompleted(Player player);
}