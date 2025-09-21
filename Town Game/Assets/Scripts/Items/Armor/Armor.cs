using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Items/Armor")]
public class Armor : Item
{
    public ClothingGroup clothingGroup;
    [Tooltip("The clothing objects for this armor. The Head clothing group needs a Hat/Mask/Head object." +
        " The Torso clothing group needs a Torso object." +
        " The Legs clothing group needs a Legs object.")]
    public Clothing[] clothing;

    public override string GetItemType()
    {
        return "Clothing";
    }
}
