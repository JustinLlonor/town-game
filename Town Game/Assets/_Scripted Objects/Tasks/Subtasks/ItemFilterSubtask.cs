using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A subtask that is complete when at least one item in the player's inventory fulfills the item filter
/// </summary>
[CreateAssetMenu(fileName = "New Item Filter Subtask", menuName = "Tasks/Subtasks/Item Filter")]
public class ItemFilterSubtask : Subtask
{
    public ItemFilter itemFilter;

    public override void OnActivateClient() { }

    public override void OnDeactivateClient() { }

    public override bool IsCompleted(Player player = null)
    {
        Item[] inventory = player.playerInventory.GetInventory();
        ItemData[] itemDatas = player.playerInventory.GetInventoryItemData();
        for (int i = 0; i < inventory.Length; i++)
        {
            Item currentItem = inventory[i];
            ItemData currentData = itemDatas[i];
            if (itemFilter.ItemIsValid(currentItem, currentData)) return true;
        }
        return false;
    }
}