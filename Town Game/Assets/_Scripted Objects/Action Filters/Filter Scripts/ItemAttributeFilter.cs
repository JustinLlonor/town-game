using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Defines a filter that allows specific item attributes
/// </summary>
[CreateAssetMenu(fileName = "New Item Attribute Filter", menuName = "Item Filters/Item Attribute Filter")]
public class ItemAttributeFilter : ItemFilter
{
    public ItemAttribute[] allowedAttributes;

    public override bool ItemIsValid(Item item, ItemData data)
    {
        foreach (ItemAttribute attribute in item.attributes)
        {
            if (allowedAttributes.Contains(attribute)) return true;
        }
        return false;
    }

    public override bool ItemIsValid(Item item, ItemData data, out FilterInfo filterCause)
    {
        filterCause = FilterInfo.None;
        bool returnVal = false;
        foreach (ItemAttribute attribute in item.attributes)
        {
            if (allowedAttributes.Contains(attribute))
            {
                if (filterCause.filteredAttributes == null) filterCause.filteredAttributes = new List<ItemAttribute>();
                filterCause.filteredAttributes.Add(attribute);
                returnVal = true;
            }
        }
        return returnVal;
    }
}
