using System;
using System.Collections.Generic;
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
        return EqualityComparer<Item>.Default.Equals(filteredItem, other.filteredItem) &&
               EqualityComparer<List<ItemAttribute>>.Default.Equals(filteredAttributes, other.filteredAttributes);
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
}