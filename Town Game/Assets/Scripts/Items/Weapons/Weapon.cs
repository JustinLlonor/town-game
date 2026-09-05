using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class Weapon : Item
{
    [Header("Weapon Settings")]
    [Tooltip("The capacity for a weapon to overpower another weapon. A value of 0 makes the weapon unusable for offense")]
    [Range(0, 10)]
    public int strength = 5;
    [Range(0, 10)]
    [Tooltip("The capacity for a weapon to resist attacks from another weapon. A value of 0 makes the weapon unusable for defense")]
    public int defense = 5;
    [Tooltip("The raycast range of the weapon")]
    public float range = 0.8f;
    [Tooltip("The name of the animation state when the victim loses an encounter to this weapon")]
    public string deathScreenAnimationState;
    public Shake shake;
    [Header("Sounds")]
    public string[] damageSounds = new string[] { };
    //[Header("Evidence")]
    //public string[] evidenceDescriptions = new string[] { };
    //public string[] evidenceIcons = new string[] { };
    //    public float headMultiplier = 1.2f;
    //    public bool disablesLegs = false;
    //    public bool concussesHead = false;

    public override string GetItemType()
    {
        return "Weapon";
    }
}
