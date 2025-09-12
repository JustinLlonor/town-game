using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the attribute for an item. Use ToReadable instead of ToString
/// </summary>
public enum ItemAttribute
{
    // Negative values are weapon attributes
    Blunt = -1,
    Sharp = -2,
    Pointy = -3,
    None = 0,
    StraightLockpick = 1,
    CurvyLockpick = 2,
    AngledLockpick = 3
}

public static class ItemAttributeExtensions
{
    private static Dictionary<ItemAttribute, string> attributeNames = new Dictionary<ItemAttribute, string>()
    {
        { ItemAttribute.StraightLockpick, "Straight Lockpick"},
        { ItemAttribute.CurvyLockpick, "Curvy Lockpick"},
        { ItemAttribute.AngledLockpick, "Angled Lockpick"}
    };

    /// <summary>
    /// Converts this item attribute to a readable string
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static string ToReadable(this ItemAttribute attribute)
    {
        // returns the regular ToString if the attribute doesn't have a custom name. Otherwise,
        // returns the attribute name from the dictionary
        if (!attributeNames.ContainsKey(attribute)) return attribute.ToString();
        return attributeNames[attribute];
    }

    /// <summary>
    /// If this item attribute is a weapon attribute or not
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static bool IsWeaponAttribute(this ItemAttribute attribute)
    {
        return (int)attribute < 0; // if it's less than 0 it is
    }
}
