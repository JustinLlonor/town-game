using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class Item : ScriptableObject
{
    public Texture2D icon; 
    public string description = "";
    // If an item is large it cannot be stored in inventory, has to be held with both hands
    public string[] equipSounds = new string[] { "Equip1", "Equip2", "Equip3" };
    public bool large = false;
    public float yOffset = -0.14f;
    public float pullSpeed = 40f;
    public float dragSpeed = 120f;
    public string holdPose;
    public Mesh model;
    public Material material;
}
