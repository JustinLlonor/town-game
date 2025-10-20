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

    /// <summary>
    /// Sets the power reception of this breaker.
    /// </summary>
    /// <param name="newPower"></param>
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
    /// Swaps two groups. Make this function have a cooldown, since it's pretty expensive
    /// </summary>
    /// <param name="groupIndex1"></param>
    /// <param name="groupIndex2"></param>
    public void SwapGroups(int groupIndex1, int groupIndex2)
    {
        if (groupIndex1 == 0 || groupIndex2 == 0) return; // Can't swap with groupless group
        int groupCount1 = GetGroupCount(groupIndex1);
        int groupCount2 = GetGroupCount(groupIndex2);
        if (groupCount1 == -1 || groupCount2 == -1) return;
        // Set biggerStartIndex to the start index of the group that has more items, and smaller to be the other.
        int biggerStartIndex;
        int smallerStartIndex;
        int smallerCount;
        int biggerCount;
        int smallerGroupIndex;
        if (groupCount1 > groupCount2)
        {
            biggerStartIndex = GetPowerPriorityStartIndex(groupIndex1);
            smallerStartIndex = GetPowerPriorityStartIndex(groupIndex2);
            biggerCount = groupCount1;
            smallerCount = groupCount2;
            smallerGroupIndex = groupIndex2;
        }
        else
        {
            biggerStartIndex = GetPowerPriorityStartIndex(groupIndex2);
            smallerStartIndex = GetPowerPriorityStartIndex(groupIndex1);
            biggerCount = groupCount2;
            smallerCount = groupCount1;
            smallerGroupIndex = groupIndex1;
        }
        // Iterate over the overlap between the smaller group and bigger group
        for (int i = 0; i < smallerCount; i++)
        {
            // The indices to swap
            int sIndex = smallerStartIndex + i;
            int bIndex = biggerStartIndex + i;
            // swap
            NetworkBehaviourId smallerBehaviour = equipmentPowerPriority.Get(sIndex);
            equipmentPowerPriority.Set(sIndex, equipmentPowerPriority.Get(bIndex));
            equipmentPowerPriority.Set(bIndex, smallerBehaviour);
        }
        // Get leftover elements to insert in the bigger list
        List<NetworkBehaviourId> insertedBehaviours = new List<NetworkBehaviourId>();
        for (int i = smallerCount; i < biggerCount; i++)
        {
            int bIndex = biggerStartIndex + i;
            insertedBehaviours.Add(equipmentPowerPriority.Get(bIndex));
        }
        // remove all leftover elements
        foreach (NetworkBehaviourId behaviour in insertedBehaviours)
        {
            equipmentPowerPriority.Remove(behaviour);
        }
        // Insert the remaining to the end of the smaller group
        int insertionIndex = GetPowerPriorityStartIndex(smallerGroupIndex) + smallerCount;
        for (int i = insertedBehaviours.Count - 1; i >= 0; i--)
        {
            InsertIntoPriority(insertionIndex, insertedBehaviours[i]);
        }
        // Swap the groups in group order
        int group1OrderIndex = GetGroupOrderIndex(groupIndex1);
        int group2OrderIndex = GetGroupOrderIndex(groupIndex2);
        Vector2Int group1Value = groupOrder.Get(group1OrderIndex);
        groupOrder.Set(group1OrderIndex, groupOrder.Get(group2OrderIndex));
        groupOrder.Set(group2OrderIndex, group1Value);
    }

    /// <summary>
    /// Swaps two equipment in the power priority list
    /// </summary>
    /// <param name="groupIndex"></param>
    /// <param name="localIndex1"></param>
    /// <param name="localIndex2"></param>
    public void SwapEquipment(int groupIndex, int localIndex1, int localIndex2)
    {
        int groupLength = GetGroupCount(groupIndex);
        if (groupLength == -1)
            throw new ArgumentException("Group index does not exist");
        if (localIndex1 >= groupLength || localIndex2 >= groupLength) 
            throw new ArgumentOutOfRangeException("Local index out of range of group");
        // get swap indices
        int startIndex = GetPowerPriorityStartIndex(groupIndex);
        int swap1 = startIndex + localIndex1;
        int swap2 = startIndex + localIndex2;
        // perform the swap
        NetworkBehaviourId value1 = equipmentPowerPriority.Get(swap1);
        equipmentPowerPriority.Set(swap1, equipmentPowerPriority.Get(swap2));
        equipmentPowerPriority.Set(swap2, value1);
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
    /// Gets the first index in a group within the power priority list
    /// </summary>
    /// <param name="groupIndex"></param>
    /// <returns></returns>
    private int GetPowerPriorityStartIndex(int groupIndex)
    {
        int returnIndex = 0;
        for (int i = 0; i < groupOrder.Count; i++)
        {
            if (groupOrder[i].x == groupIndex) break;
            returnIndex += groupOrder[i].y;
        }
        return returnIndex;
    }

    /// <summary>
    /// Gets the number of items in a certain group index
    /// </summary>
    /// <param name="groupIndex"></param>
    /// <returns></returns>
    private int GetGroupCount(int groupIndex)
    {
        for (int i = 0; i < groupOrder.Count; i++)
        {
            if (groupOrder[i].x == groupIndex) return groupOrder[i].y;
        }
        return -1;
    }

    private int GetGroupOrderIndex(int groupIndex)
    {
        for (int i = 0; i < groupOrder.Count; i++)
        {
            if (groupOrder[i].x == groupIndex) return i;
        }
        return -1;
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
