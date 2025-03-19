using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class AttackManager : NetworkBehaviour
{
    public bool isAttacking = false;
    public float animationCooldown = .1f;
    public float weightLerp = 50f;
    public Collider[] colliders;
    public LayerMask playerMask;
    public LayerMask environmentMask;
    [HideInInspector] public Animator animator;
    //[HideInInspector] public PhotonView view;
    [HideInInspector] public Transform animHolder;
    GameObject lockedPlayer;

    GameObject uiGlitch;
    PlayerInventory inventory;
    FirstPerson fps;
    CameraShake cShake;
    Transform camTransform;

    private void Awake()
    {
        cShake = FindFirstObjectByType<CameraShake>();
        fps = FindFirstObjectByType<FirstPerson>();
        UIManager um = FindFirstObjectByType<UIManager>();
        if (um != null) uiGlitch = um.glitchObject;
        inventory = gameObject.GetComponent<PlayerInventory>();
        camTransform = Camera.main.transform;
        //if (!view.IsMine) return;
        foreach(Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (HasInputAuthority)
        {
            if (uiGlitch != null) uiGlitch.SetActive(false);
        }
    }

    private void Update()
    {
        DetectLockedPlayer();
    }

    bool WeaponEquipped()
    {
        if (inventory.equippedItem == null) return false;
        if (inventory.equippedItem is Weapon) return true;
        return false;
    }

    Weapon GetWeapon()
    {
        return (Weapon)inventory.equippedItem;
    }

    // WIP, based on various conditions like night time, isCultist, etc. to determine if the player can initiate an attack.
    bool CanAttack()
    {
        return true;
    }

    // Detects locked player for the client
    void DetectLockedPlayer()
    {
        if (!HasInputAuthority) return;
        if (!CanAttack()) return;
        if (!WeaponEquipped())
        {
            ResetGlitchEffect();
            return;
        }
        Weapon currentWeapon = GetWeapon();

        // Raycasting
        if (Physics.Raycast(camTransform.position, camTransform.rotation * Vector3.forward, currentWeapon.range, (int)environmentMask))
        {
            ResetGlitchEffect();
            return;
        } // Hits environment, return
        RaycastHit hit;
        if (!Physics.Raycast(camTransform.position, camTransform.rotation * Vector3.forward, out hit, currentWeapon.range, (int)playerMask))
        {
            ResetGlitchEffect();
            return;
        } // Hits player, continue

        // Change detector
        GameObject currentPlayer = hit.transform.gameObject;
        if (currentPlayer == lockedPlayer) return;

        // After we lock onto a player
        uiGlitch.SetActive(true);
        if (lockedPlayer != null) lockedPlayer.GetComponent<Player>().DisableUIFront(); // Disable ui front for locked player
        currentPlayer.GetComponent<Player>().EnableUIFront(); // Enable for the current player

        lockedPlayer = currentPlayer;
    }

    void ResetGlitchEffect()
    {
        if (lockedPlayer == null) return;
        lockedPlayer.GetComponent<Player>().DisableUIFront();
        lockedPlayer = null;
        uiGlitch.SetActive(false);
    }

    // Server checks if the locked player is in range, then executes the attack sequence
    void AttackLockedPlayer()
    {

    }

    // Called o client and server to attack
    public void Attack(Weapon weapon)
    {
        /**
        if (atkCooldown > 0) return;
        if (isAttacking) return;
        Item.AnimationState state = weapon.useAnimations[currentAtk];
        string cState = weapon.clientAnimations[currentAtk];
        fps.PlayItemUseAnimation(cState);
        **/
    }

    public void PlayShake(Shake shake)
    {
        cShake.StartShake(shake.shakeProperties);
    }

    public void CastAttackRay(float distance, float damage, Weapon weapon = null)
    {
        
    }

    // Plays the 3rd person animation state
    //[PunRPC]
    public void AttackManagerPlay(string animation, int index)
    {
        animator.Play(animation, index, 0f);
    }
}
