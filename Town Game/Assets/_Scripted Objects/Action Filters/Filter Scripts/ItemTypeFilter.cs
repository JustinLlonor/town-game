using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Defines a filter that checks for item types
/// </summary>
[CreateAssetMenu(fileName = "New Item Type Filter", menuName = "Item Filters/Item Type Filter")]
public class ItemTypeFilter : ItemFilter
{
    public string[] allowedTypes;

    public override bool ItemIsValid(Item item, ItemData data)
    {
        return allowedTypes.Contains(item.GetItemType()); // returns true if the allowed types equal the item type
    }
}
