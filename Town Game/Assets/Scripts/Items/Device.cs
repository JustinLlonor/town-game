using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class Device : Item
{
    public override string GetType()
    {
        return "Device";
    }
}
