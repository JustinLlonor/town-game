using System.Collections.Generic;
using WebSocketSharp;

public struct FilterInfo
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