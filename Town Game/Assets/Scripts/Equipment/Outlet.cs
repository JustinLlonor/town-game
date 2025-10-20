using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outlet : NetworkBehaviour
{
    [Networked, Capacity(2)] public NetworkLinkedList<NetworkBehaviourId> equipments => default;
    public Breaker breaker;

    public bool IsFull()
    {
        return equipments.Count >= 2; // Max # of equipment is 2 
    }

    /// <summary>
    /// Adds an equipment object to this outlet
    /// </summary>
    /// <param name="equipment"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public bool AddEquipmentToOutlet(Equipment equipment)
    {
        if (!Runner.IsServer) 
            throw new InvalidOperationException("This method can only be called on the server.");
        if (IsFull()) return false; // returns if outlet is full
        equipments.Add(equipment.Id);
        breaker.AddEquipmentToPoweredList(this, equipment);
        return true;
    }

    public bool RemoveEquipmentFromOutlet(Equipment equipment)
    {
        NetworkBehaviourId eId = equipment.Id;
        if (!equipments.Contains(eId)) return false;
        equipments.Remove(eId);
        breaker.RemoveEquipmentFromPoweredList(eId);
        return true;
    }
}