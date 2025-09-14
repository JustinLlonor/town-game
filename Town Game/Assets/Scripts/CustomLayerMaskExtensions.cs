using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomLayerMaskExtensions
{
    public static void AddLayer(ref this LayerMask val, int layer)
    {
        val |= 1 << layer;
    }

    public static void RemoveLayer(ref this LayerMask val, int layer)
    {
        val ^= 1 << layer;
    }

    public static bool Contains(this LayerMask val, int layer)
    {
        return ((val & (1 << layer)) > 0);
    }
}
