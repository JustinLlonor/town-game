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
    public ItemBehaviour itemBehaviour;
    public GameObject itemComponentHolder; // TO BE REPLACED
    public bool large = false; // TO BE DEPRECATED  
    public Mesh mesh; // Mesh of the item
    public Texture2D texture;
    [Header("Sound")]
    public string[] useSounds = new string[] { };
    public string[] equipSounds = new string[] { "Equip1", "Equip2", "Equip3" };
    public bool leaveFingerprints = true; // TO BE DEPRECATED
    [Header("Usage")]
    public string useMethod; // TO BE OBSELETE
    public string secondaryUseMethod; // TO BE OBSELETE
    [Header("Client Animation")]
    public string holdPose = "ArmHoldNormal_f";
    public string gripPose;
    [Tooltip("The client sided use animations for this item. For weapons, index 0 is attack, index 1 is defense, index 2 is engagement, and index 3 is collateral attacks")]
    public string[] clientUseAnimations;
    [Header("Character Animation")]
    [Tooltip("The server sided use animations for this item. For weapons, index 0 is attack, index 1 is defense, index 2 is engagement, and index 3 is collateral attacks")]
    public AnimationState[] useAnimations;
    public AnimationState[] holdPoses;
    
    [System.Serializable]
    public struct AnimationState
    {
        public string animation;
        public string layer;
    }
}
