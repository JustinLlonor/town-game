using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class Item : ScriptableObject
{
    public Texture2D icon;
    public string description = "";
    public bool large = false; // large if cannot be stored in inventory
    public Mesh mesh;
    public Texture2D texture;
    public string[] equipSounds = new string[] { "Equip1", "Equip2", "Equip3" };
    [Header("Usage")]
    public string useMethod;
    public string secondaryUseMethod;
    [Header("Client Animation")]
    public string holdPose = "ArmHoldNormal_f";
    public string gripPose;
    public string[] clientAnimations;
    [Header("Character Animation")]
    public AnimationState[] useAnimations;
    public AnimationState[] holdPoses;
    
    [System.Serializable]
    public struct AnimationState
    {
        public string animation;
        public string layer;
    }
}
