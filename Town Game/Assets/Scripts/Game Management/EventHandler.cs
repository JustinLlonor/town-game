using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles certain events in the schedule system
/// </summary>
public class EventHandler : MonoBehaviour
{
    public List<PlayerRef> subscribedPlayers = new List<PlayerRef>();
    public Color defaultBlockColor = Color.white;
    public List<int> interestGroups = null;
    ScheduleManager scheduleManager;

    private void Awake()
    {
        scheduleManager = FindAnyObjectByType<ScheduleManager>();
    }

    public void CreateEvent(string eventName, string room, float time, float length)
    {
        List<int> interestGroups = null;
        if (this.interestGroups != null)
        {
            interestGroups = new List<int>(this.interestGroups.ToArray());
        }
        scheduleManager.AddBlock(eventName, room, time, length, defaultBlockColor, subscribedPlayers, interestGroups);
    }

    public void CreateEvent(string eventName, string room, float time, float length, Color blockColor)
    {
        List<int> interestGroups = null;
        if (this.interestGroups != null)
        {
            interestGroups = new List<int>(this.interestGroups.ToArray());
        }
        scheduleManager.AddBlock(eventName, room, time, length, blockColor, subscribedPlayers, interestGroups);
    }

    public void AddSubscription(PlayerRef player)
    {
        if (subscribedPlayers.Contains(player)) return;
        subscribedPlayers.Add(player);
    }

    public void RemoveSubscription(PlayerRef player)
    {
        if (!subscribedPlayers.Contains(player)) return;
        subscribedPlayers.Remove(player);
    }
}
