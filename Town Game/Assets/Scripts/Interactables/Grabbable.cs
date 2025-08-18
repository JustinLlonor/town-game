using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Fusion;

public class Grabbable : NetworkBehaviour
{
    [Networked] public bool canGrab { get; set; } = true;
    [Networked] public Vector3 grabPoint { get; set; }
    [Networked] public PlayerRef grabber { get; set; } = PlayerRef.None;
    /// <summary>
    /// Called when checking if a grab is valid. The grab is invalid if all functions return false.
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
            // if at least one of the check delegates is true, then return true
            Delegate[] checkDelegates = playerCheck.GetInvocationList();
            for (int i = 0; i < checkDelegates.Length; i++)
            {
                bool checkValid = (bool)checkDelegates[i].DynamicInvoke(player);
                if (checkValid) return true;
            }
            return false;
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
