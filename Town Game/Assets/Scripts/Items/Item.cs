using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class Item : ScriptableObject
{
    public Texture2D icon;
    public string description = "";
    public bool large = false; // large if cannot be stored in inventory
    public Mesh model;
    public Material material;
    public string[] equipSounds = new string[] { "Equip1", "Equip2", "Equip3" };
    [Header("Usage")]
    public AnimationState[] useAnimations;
    public AnimationState[] clientAnimations;
    public string useMethod;
    public string secondaryUseMethod;
    [Header("Animation")]
    public float yOffset = -0.14f;
    public float iYOffset = 0.1f;
    public float angleOffset = 30f;
    public float pullSpeed = 40f;
    public float dragSpeed = 120f;
    public AnimationState[] holdPoses;
    
    [System.Serializable]
    public struct AnimationState
    {
        public string animation;
        public string layer;
    }
}
