using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class AttackManager : NetworkBehaviour
{
    public Collider[] colliders; // DO NOT DELETE YET, use damage colliders for better player interactable  hover
    public LayerMask playerMask;
    public LayerMask environmentMask;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Transform animHolder;
    public Rigidbody rb;
    public float minSliderSpeed = 350f;
    public float maxSliderSpeed = 700f;
    public float minTargetLength = 50f;
    public float maxTargetLength = 300f;
    GameObject lockedPlayer;

    //Engagement
    [Networked] public bool isEngaged { get; set; } = false;
    bool previouslyEngaged = false;
    bool isAttacker;

    Player player;
    GameObject uiGlitch;
    PlayerInventory inventory;
    FirstPerson fps;
    CameraShake cShake;
    ConflictManager conflictManager;
    PlayerManager playerManager;
    InputManager inputManager;
    UIManager uiManager;
    AttackQTE attackQTE;

    Transform camTransform;

    public void Init()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        attackQTE = FindFirstObjectByType<AttackQTE>();
        attackQTE = uiManager.attackQTE;
        inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onJump += EngagementQTE;
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

    public override void Spawned()
    {
        Init();
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
        DetectEngagedChange();
    }

    void DetectEngagedChange()
    {
        if (previouslyEngaged == isEngaged) return;
        previouslyEngaged = isEngaged; // When there is a change
        OnEngagedChange();
    }

    void OnEngagedChange()
    {
        if (!isEngaged)
        {
            ResetGlitchEffect();
        }
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
        RaycastHit envHit;
        RaycastHit playerHit;
        bool isEnv = Physics.Raycast(camTransform.position, camTransform.rotation * Vector3.forward, out envHit, currentWeapon.range, (int)environmentMask);
        bool isPlayer = Physics.Raycast(camTransform.position, camTransform.rotation * Vector3.forward, out playerHit, currentWeapon.range, (int)playerMask);
        if (isEnv && isPlayer) // If a raycast hit both an environment and player
        {
            if (envHit.distance < playerHit.distance) // If the environment is in front of the player, return and reset glitch effect
            {
                ResetGlitchEffect();
                return;
            }
        }
        if (!isPlayer)
        {
            ResetGlitchEffect();
            return;
        }

        // Change detector
        GameObject currentPlayer = playerHit.transform.gameObject;
        if (currentPlayer == lockedPlayer) return;
        if (currentPlayer == gameObject) return;

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
        RaycastHit envHit;
        RaycastHit playerHit;
        bool isEnv = Physics.Raycast(castPosition, castDirection, out envHit, currentWeapon.range, (int)environmentMask);
        bool isPlayer = Physics.Raycast(castPosition, castDirection, out playerHit, currentWeapon.range, (int)playerMask);
        if (isEnv && isPlayer)
        {
            if (envHit.distance < playerHit.distance) return;
        }
        if (!isPlayer) return;

        // If we get a hit, check the victim and then start engagement 
        PlayerRef victim = playerHit.transform.GetComponent<Player>().owner; // Gets the victim 
        if (victim == player.owner) return; // Can't hit self
        if (!playerManager.playerObjects.ContainsKey(victim)) return;

        conflictManager.StartEngagement(player.owner, victim, currentWeapon); // Start an engagement with the owner and the victim

        // Raycast to victim object
    }

    /// <summary>
    /// Linked to onJump, the function for determining if this client won a quicktime event
    /// </summary>
    void EngagementQTE()
    {
        if (!HasInputAuthority) return;
        if (!isEngaged) return;
        if (isAttacker) return;
        if (!attackQTE.enabled) return;
        if (!attackQTE.GetSliderSuccess()) return;
        RPC_WonQTE();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_WonQTE()
    {
        conflictManager.WonQuicktime(Object.InputAuthority);
    }

    /// <summary>
    /// Starts the engagement sequence
    /// </summary>
    /// <param name="player"></param>
    /// <param name="isAttacker"></param>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_StartEngagementSequence([RpcTarget] PlayerRef player, bool isAttacker, int attack, int defense)
    {
        this.isAttacker = isAttacker;
        if (isAttacker)
        {
            // Play attack animation, attack sequence
        }
        else
        {
            if (defense != 0)
            {
                attackQTE.gameObject.SetActive(true);
                float potency = Mathf.Clamp((float)(attack - defense + 1), 0f, 10f) / 10f;
                float sliderSpeed = (maxSliderSpeed - minSliderSpeed) * potency + minSliderSpeed;
                float targetLength = (maxTargetLength - minTargetLength) * potency + minTargetLength;
                attackQTE.Init(sliderSpeed, targetLength); // Calculate this based on attack and defense later
            }
            // Play defense animation, defense sequence
        }
        uiManager.ExitUI();
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
