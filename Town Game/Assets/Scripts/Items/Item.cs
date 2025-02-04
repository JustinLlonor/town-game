using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class Item : ScriptableObject
{
    [Header("Info")]
    public Texture2D icon;
    public string description = "";
    public Vector3 placedRotation = Vector3.zero;
    public GameObject itemComponentHolder;
    public bool large = false; // large if cannot be stored in inventory
    public Mesh mesh; // Mesh of the item
    public Texture2D texture;
    public string[] equipSounds = new string[] { "Equip1", "Equip2", "Equip3" };
    public bool leaveFingerprints = true;
    [Header("Usage")]
    public string useMethod; // Make the use methods able to gain information from the player, as well as
    public string secondaryUseMethod; // access some sort of animation system for the client/character
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
