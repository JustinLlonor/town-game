using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Defines an item filter that allows specific items
/// </summary>
[CreateAssetMenu(fileName = "New Specific Item Filter", menuName = "Item Filters/Specific Item Filter")]
public class SpecificItemFilter : ItemFilter
{
    public Item[] allowedItems;

    public override bool ItemIsValid(Item item, ItemData data)
    {
        return allowedItems.Contains(item); // Returns true if the item is contained within allowed items
    }
}
