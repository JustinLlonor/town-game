using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Fusion;
using System.Linq;

public class Breaker : NetworkBehaviour
{
    /// <summary>
    /// The amount of power this breaker receives. Set this with the SetPowerReception function.
    /// </summary>
    [Networked] public float powerReception { get; set; }
    public Outlet[] outlets;
    /// <summary>
    /// Defines the priority that each equipment holds. Higher priorities get power first.
    /// The x value is the outlet index, the y value is the equipment index.
    /// </summary>
    [Networked, Capacity(30)] public NetworkLinkedList<NetworkBehaviourId> equipmentPowerPriority => default;

    public override void Spawned()
    {
        foreach (Outlet outlet in outlets)
        {
            outlet.breaker = this;
        }
    }

    public void AddEquipmentToPriority(Outlet outlet, Equipment equipment)
    {
        if (!outlets.Contains(outlet)) return;
        equipmentPowerPriority.Add(equipment.Id);
    }

    public void RemoveEquipmentFromPriority(NetworkBehaviourId id)
    {
        if (!equipmentPowerPriority.Contains(id)) return;
        equipmentPowerPriority.Remove(id);
    }

    public void SetPowerReception(float newPower)
    {
        powerReception = newPower;
        UpdateEquipmentPowered();
    }

    /// <summary>
    /// Updates the powered state of connected equipment depending on the value of this breaker's power reception
    /// </summary>
    private void UpdateEquipmentPowered()
    {
        int poweredIndex = 0;
        float remainingPower = powerReception;
        // Sets equipment to powered
        while (remainingPower > 0f && poweredIndex < equipmentPowerPriority.Count)
        {
            Equipment foundEquipment = GetEquipmentFromRef(equipmentPowerPriority[poweredIndex]);
            remainingPower -= foundEquipment.energyConsumption;
            foundEquipment.powered = true;
            poweredIndex++;
        }
        // Depowers remaining equipment
        while (poweredIndex < equipmentPowerPriority.Count)
        {
            Equipment foundEquipment = GetEquipmentFromRef(equipmentPowerPriority[poweredIndex]);
            foundEquipment.powered = false;
            poweredIndex++;
        }
    }

    /// <summary>
    /// Gets equipment object from the ref
    /// </summary>
    /// <param name="eRef"></param>
    /// <returns></returns>
    private Equipment GetEquipmentFromRef(NetworkBehaviourId eRef)
    {
        NetworkBehaviour foundBehaviour;
        Runner.TryFindBehaviour(eRef, out foundBehaviour);
        if (foundBehaviour != null && foundBehaviour is Equipment)
        {
            return (Equipment)foundBehaviour;
        }
        return null;
    }
}
