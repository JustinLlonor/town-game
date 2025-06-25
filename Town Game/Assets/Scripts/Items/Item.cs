using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class Item : ScriptableObject
{
    [Header("Info")]
    public Texture2D icon;
    public string description = "";
    public string customType = "";
    public GizmoSettings dropSettings;
    public Vector3 placedRotation = Vector3.zero;
    public GameObject itemBehaviourObject;
    public Mesh mesh; // Mesh of the item
    public Texture2D texture;
    [Tooltip("If this is set, the material will be used on the placed item instead of the texture")]
    public Material material;
    [Header("Sound")]
    public string[] useSounds = new string[] { };
    public string[] equipSounds = new string[] { "Equip1", "Equip2", "Equip3" };
    [Header("Client Animation")]
    public string holdPose = "ArmHoldNormal_f";
    public string gripPose;
    [Tooltip("The client sided use animations for this item. For weapons, index 0 is attack, index 1 is defense, index 2 is engagement, and index 3 is collateral attacks")]
    public string[] clientUseAnimations; // TO IMPLEMENT
    [Header("Character Animation")]
    [Tooltip("The server sided use animations for this item. For weapons, index 0 is attack, index 1 is defense, index 2 is engagement, and index 3 is collateral attacks")]
    public AnimationState[] useAnimations; // TO IMPLEMENT
    public AnimationState[] holdPoses;
    
    [System.Serializable]
    public struct AnimationState
    {
        public string animation;
        public string layer;
    }

    public virtual string GetItemType()
    {
        if (customType.IsNullOrEmpty()) return "Item";
        return customType;
    }
}
