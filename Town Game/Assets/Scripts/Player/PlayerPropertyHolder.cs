using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerPropertyHolder : NetworkBehaviour
{
    /// <summary>
    /// The player's nickname
    /// </summary>
    [Networked] public NetworkString<_32> nickname { get; set; }
    /// <summary>
    /// If the player is a cultist or not
    /// </summary>
    [Networked] public bool isCultist { get; set; }
    /// <summary>
    /// The home of the player
    /// </summary>
    [Networked] public int room { get; set; }
    /// <summary>
    /// Duh
    /// </summary>
    [Networked] public float money { get; set; }
    /// <summary>
    /// Groups the player is in for the schedule system
    /// </summary>
    [Networked, Capacity(10)] public NetworkLinkedList<int> groups => default;
    /// <summary>
    /// Soon to be obselete energy system for player visits
    /// </summary>
    [Networked] public int energy { get; set; }
    /// <summary>
    /// The keys of the rooms the player has access to
    /// </summary>
    [Networked, Capacity(20)] public NetworkLinkedList<int> keys => default;
    /// <summary>
    /// The devices the player has placed
    /// </summary>
    [Networked, Capacity(30)] public NetworkLinkedList<NetworkId> devices => default;
}
