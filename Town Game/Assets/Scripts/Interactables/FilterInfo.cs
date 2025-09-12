using System.Collections.Generic;
using WebSocketSharp;

public struct FilterInfo
{
    public Item filteredItem;
    public List<ItemAttribute> filteredAttributes;
    public string filteredType;

    public FilterInfo(Item filteredItem)
    {
        this.filteredItem = filteredItem;
        filteredAttributes = null;
        filteredType = null;
    }

    public FilterInfo(List<ItemAttribute> filteredAttributes)
    {
        this.filteredAttributes = filteredAttributes;
        filteredItem = null;
        filteredType = null;
    }

    public FilterInfo(string filteredType)
    {
        this.filteredType = filteredType;
        filteredAttributes = null;
        filteredItem = null;
    }

    public static FilterInfo None
    {
        get
        {
            FilterInfo info = new FilterInfo();
            info.filteredItem = null;
            info.filteredAttributes = null;
            info.filteredType = null;
            return info;
        }
    }
}

public static class FilterInfoExtensions
{
    public static bool IsNone(this FilterInfo info)
    {
        if (info.filteredItem != null) return false;
        if (info.filteredAttributes != null) return false;
        if (!info.filteredType.IsNullOrEmpty()) return false;
        return true;
    }
}