using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "New Device", menuName = "Items/Device")]
public class Device : Item
{
    public GizmoSettings devicePlacementSettings;
    public NetworkPrefabRef devicePrefab;

    public override string GetItemType()
    {
        return "Device";
    }
}
