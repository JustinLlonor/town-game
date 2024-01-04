using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class Weapon : Item
{
    [Header("Weapon Settings")]
    public float damage = 20f;
    public float range = 0.8f;
    public float attackCooldown = 1f;
    public string[] attackSounds = new string[] { };
    public string[] damageSounds = new string[] { };
    public float headMultiplier = 1.2f;
    public bool disablesLegs = false;
    public bool concussesHead = false;
}
