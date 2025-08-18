using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Grabbable : NetworkBehaviour
{
    [Networked] public bool canGrab { get; set; } = true;
    [Networked] public Vector3 grabPoint { get; set; }
    [Networked] public PlayerRef grabber { get; set; } = PlayerRef.None;
    /// <summary>
    /// Called when checking if a grab is valid. Use a Player as a parameter, and return a bool. 
    /// Returning true means that the grab is valid, returning false means otherwise.
    /// </summary>
    public PlayerCheck playerCheck;

    public delegate bool PlayerCheck(Player player);

    /// <summary>
    /// Checks if the grab is valid or not for this particular player.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool GrabIsValid(Player player)
    {
        if (playerCheck != null)
        {
            return playerCheck.Invoke(player);
        }
        return true;
    }

    public void EnableGrab()
    {
        canGrab = true;
    }

    public void DisableGrab()
    {
        canGrab = false;
    }
}
