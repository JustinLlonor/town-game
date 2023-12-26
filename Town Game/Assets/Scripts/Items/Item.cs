using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class Item : ScriptableObject
{
    public string description = "";
    public float yOffset = -0.14f;
    // If an item is large it cannot be stored in inventory, has to be held with both hands
    public bool large = false;
    public string[] equipSounds = new string[] { "Equip1", "Equip2", "Equip3" };
    public Mesh model;
    public Material material;
    public float lerp = 40f;
}
