using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Metadata Filter", menuName = "Item Filters/Item Metadata Filter")]
public class ItemMetadataFilter : ItemFilter
{
    public MetadataCheck check;

    [System.Serializable]
    public struct MetadataCheck
    {
        public string tag;
        public int value;
        public Comparison comparison;

        public enum Comparison
        {
            Equals = 0,
            NotEquals = 1,
            GreaterThan = 2,
            LessThan = 3,
        }
    }

    public override bool ItemIsValid(Item item, ItemData data)
    {
        if (!data.metadata.ContainsKey(check.tag)) return false;
        int value = data.metadata[check.tag];
        // using the comparisons, check the item value on the left compared to the check value on the right
        switch (check.comparison)
        {
            case MetadataCheck.Comparison.Equals:
                return value == check.value;
            case MetadataCheck.Comparison.NotEquals:
                return value != check.value;
            case MetadataCheck.Comparison.GreaterThan:
                return value > check.value;
            case MetadataCheck.Comparison.LessThan:
                return value < check.value;
            default:
                return false;
        }
    }
}
