using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class Weapon : Item
{
    [Header("Weapon Settings")]
    public float damage = 20f;
    public float range = 0.8f;
    public float attackLength = 1f;
    [Tooltip("Amount of time for the attack raycast to be sent")]
    public float attackCharge = 0.44f;
    [Tooltip("How long before the player can attack again")]
    public float attackCooldown = .44f;
    public Shake shake;
    public string[] attackSounds = new string[] { };
    public string[] damageSounds = new string[] { };
    public string[] evidenceDescriptions = new string[] { };
    public string[] evidenceIcons = new string[] { };
//    public float headMultiplier = 1.2f;
//    public bool disablesLegs = false;
//    public bool concussesHead = false;
}
