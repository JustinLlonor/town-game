using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerEventManager : MonoBehaviour
{
    public delegate void PlayerEvent(PlayerRef player);

    public class PlayerEvents
    {
        /// <summary>
        /// Called when a player joins
        /// </summary>
        public PlayerEvent onPlayerJoin;
        /// <summary>
        /// Called when any player leaves
        /// </summary>
        public PlayerEvent onPlayerLeave;
        /// <summary>
        /// Called when an alive player is removed, either by dying or by leaving
        /// </summary>
        public PlayerEvent onPlayerRemove;
    }
}
