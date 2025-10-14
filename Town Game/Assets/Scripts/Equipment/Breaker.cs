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
    /// <summary>
    /// The groups defined inside of the equipment power priority. x is the group index, y is the count.
    /// </summary>
    [Networked, Capacity(10)] public NetworkLinkedList<Vector2Int> groupOrder => default;
    /// <summary>
    /// All outlet groups must have the groupless group by default
    /// </summary>
    public OutletGroup[] outletGroups = new OutletGroup[] { new OutletGroup("Groupless", new List<Outlet>()) };

    [System.Serializable]
    public class OutletGroup
    {
        public string name;
        public List<Outlet> outlets;

        public OutletGroup(string name, List<Outlet> outlets)
        {
            this.name = name;
            this.outlets = outlets;
        }
    }

    public override void Spawned()
    {
        if (outletGroups[0].name != "Groupless") Debug.LogError("Groupless outlet group does not exist on this object!");
        foreach (Outlet outlet in outlets)
        {
            outlet.breaker = this;
        }
        int i = 0;
        foreach (OutletGroup group in outletGroups)
        {
            groupOrder.Add(new Vector2Int(i, group.outlets.Count));
            i++;
        }
    }

    public void SwapGroups(int groupIndex1, int groupIndex2)
    {
        if (groupIndex1 == 0 || groupIndex2 == 0) return; // Can't swap with groupless group
    }

    public void SwapEquipment(int groupId, int localIndex1, int localIndex2)
    {

    }

    /// <summary>
    /// Adds the specified equipment to the equipment power priority list based on the outlet group.
    /// </summary>
    /// <param name="outlet"></param>
    /// <param name="equipment"></param>
    public void AddEquipmentToPoweredList(Outlet outlet, Equipment equipment)
    {
        if (!outlets.Contains(outlet)) return;
        int groupIndex = GetGroupIndex(outlet);
        // Gets the index we should insert into the list in relation to the group
        int insertionIndex = 0;
        for (int i = 0; i < groupOrder.Count; i++)
        {
            Vector2Int group = groupOrder[i];
            insertionIndex += group.y;
            if (group.x == groupIndex)
            {
                group = new Vector2Int(group.x, group.y + 1);
                groupOrder.Set(i, group);
                break;
            }
        }
        // Inserts
        InsertIntoPriority(insertionIndex, equipment.Id);
    }

    public void RemoveEquipmentFromPoweredList(NetworkBehaviourId id)
    {
        if (!equipmentPowerPriority.Contains(id)) return;

        // Decrease the group member count by 1
        int currentGroup = GetOrderGroup(id);
        Vector2Int foundGroup = groupOrder.Get(currentGroup);
        foundGroup = new Vector2Int(foundGroup.x, foundGroup.y - 1);
        groupOrder.Set(currentGroup, foundGroup);

        // Remove the actual element
        equipmentPowerPriority.Remove(id);
    }

    /// <summary>
    /// Gets the index of the group in groupOrder containing the equipment behaviour
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private int GetOrderGroup(NetworkBehaviourId id)
    {
        int currentGroup = 0;
        int changeIndex = groupOrder.Get(currentGroup).y;
        for (int i = 0; i < equipmentPowerPriority.Count; i++)
        {
            while (i >= changeIndex)
            {
                currentGroup++;
                if (currentGroup >= groupOrder.Count) return -1;
                changeIndex += groupOrder.Get(currentGroup).y;
            }
            if (equipmentPowerPriority.Get(i) == id)
            {
                return currentGroup;
            }
        }
        return -1;
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

    /// <summary>
    /// Gets the outlet group that has this name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public OutletGroup GetOutletGroup(string name)
    {
        foreach (OutletGroup group in outletGroups)
        {
            if (group.name == name)
            {
                return group;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the group index of a specified outlet
    /// </summary>
    /// <param name="outlet"></param>
    /// <returns></returns>
    private int GetGroupIndex(Outlet outlet)
    {
        int returnIndex = 0;
        for (int i = 1; i < outletGroups.Length; i++)
        {
            if (outletGroups[i].outlets.Contains(outlet))
            {
                returnIndex = i;
                break;
            }
        }
        return returnIndex;
    }

    /// <summary>
    /// Inserts into the priority list
    /// </summary>
    /// <param name="index"></param>
    /// <param name="id"></param>
    private void InsertIntoPriority(int index, NetworkBehaviourId id)
    {
        if (index >= equipmentPowerPriority.Count || equipmentPowerPriority.Count == 0)
        {
            equipmentPowerPriority.Add(id);
            return;
        }
        if (index < 0) index = 0;
        // Expands the list
        equipmentPowerPriority.Add(NetworkBehaviourId.None);
        // set to last index
        int changeIndex = equipmentPowerPriority.Count - 1;
        while (changeIndex > index)
        {
            equipmentPowerPriority.Set(changeIndex, equipmentPowerPriority.Get(changeIndex - 1));
            changeIndex--;
        }
        // When changeIndex equals index
        equipmentPowerPriority.Set(index, id);
    }
}
