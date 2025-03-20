using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class AttackManager : NetworkBehaviour
{

    public Collider[] colliders; // DO NOT DELETE YET, use damage colliders for better player interactable  hover
    public LayerMask playerMask;
    public LayerMask environmentMask;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Transform animHolder;
    public Rigidbody rb;
    GameObject lockedPlayer;

    //Engagement
    [Networked] public bool isEngaged { get; set; } = false;
    bool previouslyEngaged = false;


    Player player;
    GameObject uiGlitch;
    PlayerInventory inventory;
    FirstPerson fps;
    CameraShake cShake;
    ConflictManager conflictManager;
    PlayerManager playerManager;

    Transform camTransform;

    private void Awake()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
        cShake = FindFirstObjectByType<CameraShake>();
        fps = FindFirstObjectByType<FirstPerson>();
        player = gameObject.GetComponent<Player>();
        conflictManager = FindFirstObjectByType<ConflictManager>();
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

    void DetectEngagedChange()
    {
        if (previouslyEngaged == isEngaged) return;
        previouslyEngaged = isEngaged; // When there is a change
        OnEngagedChange();
    }

    void OnEngagedChange()
    {

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
        if (isEngaged) return false;
        return true;
    }

    // Detects locked player for the client
    void DetectLockedPlayer()
    {
        if (!HasInputAuthority) return;
        if (!CanAttack())
        {
            ResetGlitchEffect();
            return;
        }
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

    // Server calculates attack
    public void Attack(Weapon weapon)
    {
        // Put client sided stuff here
        Debug.Log("1");
        if (!Runner.IsServer) return;
        TryEngagement(); // Cast a ray on the server

        /**
        if (atkCooldown > 0) return;
        if (isAttacking) return;
        Item.AnimationState state = weapon.useAnimations[currentAtk];
        string cState = weapon.clientAnimations[currentAtk];
        fps.PlayItemUseAnimation(cState);
        **/
    }

    public void TryEngagement()
    {
        if (!CanAttack()) return;
        if (!WeaponEquipped()) return;

        InteractableFinder inf = player.inf;
        Vector3 castPosition = new Vector3(rb.position.x, inf.trackedTransform.position.y, rb.position.z);
        Vector3 castDirection = inf.forwardDirection;

        Weapon currentWeapon = GetWeapon();
        // Raycasting
        if (Physics.Raycast(castPosition, castDirection, currentWeapon.range, (int)environmentMask)) return; // if hit 
        RaycastHit hit;
        if (!Physics.Raycast(castPosition, castDirection, out hit, currentWeapon.range, (int)playerMask)) return; // if no hit player

        // If we get a hit, check the victim and then start engagement 
        PlayerRef victim = hit.transform.GetComponent<Player>().owner; // Gets the victim 
        if (victim == player.owner) return; // Can't hit self
        if (!playerManager.playerObjects.ContainsKey(victim)) return;

        conflictManager.StartEngagement(player.owner, victim, currentWeapon); // Start an engagement with the owner and the victim

        // Raycast to victim object
    }

    /// <summary>
    /// Plays the first person attack animation for the client
    /// </summary>
    /// <param name="player"></param>
    /// <param name="text"></param>
    /// <param name="lifespan"></param>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_PlayAttackAnimation([RpcTarget] PlayerRef player)
    {

    }

    /// <summary>
    /// Plays the first person defense animation for the client
    /// </summary>
    /// <param name="player"></param>
    /// <param name="text"></param>
    /// <param name="lifespan"></param>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_PlayDefenseAnimation([RpcTarget] PlayerRef player)
    {

    }

    public void PlayShake(Shake shake)
    {
        cShake.StartShake(shake.shakeProperties);
    }

    // Plays the 3rd person animation state
    //[PunRPC]
    public void AttackManagerPlay(string animation, int index)
    {
        animator.Play(animation, index, 0f);
    }
}
