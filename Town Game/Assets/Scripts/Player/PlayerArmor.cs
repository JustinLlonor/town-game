using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Takes the information from player inventory and puts it in clothing
/// </summary>
public class PlayerArmor : NetworkBehaviour
{
    public PlayerInventory inventory;
    public PlayerClothing clothing;
    [Networked, Capacity(6)] public NetworkArray<int> defaultAttires { get; } = MakeInitializer(new int[] { -1, -1, -1, -1, -1 });
    private int armorLength;
    Item[] armors;
    public bool init = false; // set when player clothing declares default attier

    public override void Spawned()
    {
        armorLength = inventory.items.Capacity - inventory.hotbarLength;
        armors = new Item[armorLength];
    }

    public override void FixedUpdateNetwork()
    {
        if (!init) return;
        CheckArmor();
    }

    /// <summary>
    /// Finds changes in the armor, and set clothing accordingly when there is a change
    /// </summary>
    private void CheckArmor()
    {
        for (int i = -1; i >= -armorLength; i--)
        {
            int armorIndex = (-i) - 1;
            Item currentItem = inventory.GetItemAtSlot(i);
            if (currentItem != armors[armorIndex])
            {
                armors[armorIndex] = currentItem;
                if (currentItem == null)
                {
                    SetDefaultClothing(inventory.GetClothingGroup(i));
                    continue;
                } 
                if (!(currentItem as Armor)) continue;
                SetArmor((Armor)currentItem);
            }
        }
    }

    /// <summary>
    /// Resets the body parts of a clothing group
    /// </summary>
    /// <param name="group"></param>
    private void SetDefaultClothing(ClothingGroup group)
    {
        Debug.LogError("setting default");
        Clothing.BodyPart[] parts = group.GetBodyParts();
        foreach (Clothing.BodyPart part in parts)
        {
            clothing.ResetBodyPart(part, defaultAttires.ToArray());
        }
    }

    /// <summary>
    /// Sets the armor of this object
    /// </summary>
    /// <param name="armor"></param>
    private void SetArmor(Armor armor)
    {
        Debug.LogError("setting armor");
        foreach (Clothing clothingObj in armor.clothing)
        {
            clothing.SetClothing(clothingObj.name);
        }
    }
}
