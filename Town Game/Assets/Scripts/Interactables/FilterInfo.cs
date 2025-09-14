using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using WebSocketSharp;

public struct FilterInfo : IEquatable<FilterInfo>
{
    public Item filteredItem;
    public List<ItemAttribute> filteredAttributes;

    public FilterInfo(Item filteredItem)
    {
        this.filteredItem = filteredItem;
        filteredAttributes = null;
    }

    public FilterInfo(List<ItemAttribute> filteredAttributes)
    {
        this.filteredAttributes = filteredAttributes;
        filteredItem = null;
    }

    public static FilterInfo None
    {
        get
        {
            FilterInfo info = new FilterInfo();
            info.filteredItem = null;
            info.filteredAttributes = null;
            return info;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is FilterInfo info && Equals(info);
    }

    public bool Equals(FilterInfo other)
    {
        return filteredItem == other.filteredItem &&
               filteredAttributes.AttributeListEquals(other.filteredAttributes);
    }

    public static bool operator ==(FilterInfo left, FilterInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FilterInfo left, FilterInfo right)
    {
        return !(left == right);
    }
}

public static class FilterInfoExtensions
{
    public static bool IsNone(this FilterInfo info)
    {
        if (info.filteredItem != null) return false;
        if (info.filteredAttributes != null) return false;
        return true;
    }

    public static bool AttributeListEquals(this List<ItemAttribute> firstList, List<ItemAttribute> secondList)
    {
        if (firstList == null && secondList == null) return true;
        if (firstList == null || secondList == null) return false;
        if (firstList.Count != secondList.Count) return false;
        foreach (var item in firstList)
        {
            if (!secondList.Contains(item)) return false;
        }
        return true;
    }
}