using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class Weapon : Item
{
    [Header("Weapon Settings")]
    public float damage = 20f;
    public float headMultiplier = 1.2f;
    public float attackCooldown = 1f;
    public bool disablesLegs = false;
    public bool concussesHead = false;
    public Collider hitbox;
    public string[] attackSounds = new string[] { };
    public string[] damageSounds = new string[] { };
}
