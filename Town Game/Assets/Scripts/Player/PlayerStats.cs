using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public PlayerNodes playerNodes;
    [Header("HP")]
    public float maxHP = 100f;
    [Networked] public float HP { get; set; } = 100f;
    private float previousHP = 100f;
    [Header("Stamina")]
    public float maxStamina = 100f;
    [Networked] public float stamina { get; set; } = 100f;
    [SerializeField] float staminaRegenSpeed = 20f;
    [SerializeField] float regenCooldownPoint = 1f;
    [Networked] public float staminaCooldown { get; set; }
    public float staminaRegenCooldown = .5f;
    public bool canRegenStamina = true;
    [Header("Death")]
    public NetworkPrefabRef corpsePrefab;
    public Transform myRig;
    [Header("Hurt")]
    public string hurtLayer = "Hurt";
    public float hurtWeight = 0.1f;
    public float hurtLerp = 50f;
    public Shake softHurt;
    public Shake hardHurt;
    public float softThreshold = 30f;
    private ChangeDetector changeDetector;

    public delegate void IntEvent(int value);
    public delegate void StringEvent(string value);
    public delegate void StatsEvent();
    // Server events, executed on the server
    public IntEvent onHPChange;
    public IntEvent onHungerChange;
    public StatsEvent onDeath;
    // Client events
    public IntEvent onHPChangeClient;
    public IntEvent onHungerChangeClient;

    CameraShake shake;
    //PhotonView view;
    [HideInInspector] public PlayerMovement pm;
    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerInfoView piv;
    GameManager gameManager;
    ObjectManager objectManager;

    private void Start()
    {
        shake = FindFirstObjectByType<CameraShake>();
        //pm = gameObject.GetComponent<PlayerMovement>();
        //view = gameObject.GetComponent<PhotonView>();
        //if (!view.IsMine) return;
        //if (FindObjectOfType<GameManager>() != null)AddAffector(hungerAffecter);
    }

    public override void Spawned()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        objectManager = FindAnyObjectByType<ObjectManager>();
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        pm.SetGrounded();
        stamina = 100f;
        staminaCooldown = 0f;
        previousHP = HP;
    }

    public override void Render()
    {
        /**
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(HP):
                    onHPChangeClient?.Invoke(HP - previousHP);
                    previousHP = HP;
                    break;
                case nameof(hunger):
                    onHungerChangeClient?.Invoke(hunger - previousHunger);
                    previousHunger = hunger;
                    break;
            }
        }
        **/
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority) ClientDeath();
    }

    public override void FixedUpdateNetwork()
    {
        GetHP();
        CheckDeath();
        piv.recievedHP = HP;
        if (staminaCooldown > 0f) staminaCooldown -= Runner.DeltaTime;
        if (staminaRegenCooldown > 0f) staminaRegenCooldown -= Runner.DeltaTime;
        if (staminaCooldown <= regenCooldownPoint && canRegenStamina && pm.isGrounded && staminaRegenCooldown <= 0f)
        {
            RegenStamina();
        }
    }

    public void Damage(float damage)
    {
        playerNodes.ChangeNodeValue("Health", -damage);
    }

    private void GetHP()
    {
        Debug.Log(playerNodes.GetNode("Health").infoIndex);
        HP = playerNodes.GetNode("Health").value;
    }

    /// <summary>
    /// Kills the player and destroys the NetworkObject
    /// </summary>
    public void Kill()
    {
        if (!HasStateAuthority) return;
        PlayerClothing pc = gameObject.GetComponent<PlayerClothing>();
        Rigidbody rb = GetComponent<Rigidbody>();
        Quaternion newRot = Quaternion.Euler(0f, GetComponent<Player>().camDirection, 0f);
        NetworkObject corpse = Runner.Spawn(corpsePrefab, rb.position, newRot, null, (runner, o) =>
        {
            o.GetComponent<Corpse>().Init(rb.velocity, pc.isMale);
        });

        PlayerClothing cpc = corpse.GetComponent<PlayerClothing>(); // Corpse player clothing
        foreach (PlayerClothing.Attire attire in pc.attires)
        {
            if (attire.clothing == null) continue;
            cpc.SetClothing(attire.clothing.name);
        }

        //Ragdoller ragdoller = corpse.GetComponent<Ragdoller>();
        //ragdoller.SetPositionsToTarget(myRig);

        // Set corpse nickname
        //co.SetCorpseData(view.Owner);
        //corpseView.RPC("SetCorpseData", RpcTarget.OthersBuffered, view.Owner);

        // Sets corpse clothing to this player's clothing
        onDeath?.Invoke();
        Runner.Despawn(Object); // Destroy player object
    }

    /// <summary>
    /// Checks if the player is qualified for death
    /// </summary>
    void CheckDeath()
    {
        if (!Runner.IsServer) return;
        if (HP <= 0f)
        {
            Kill();
        }
    }

    /// <summary>
    /// Called on the client when there is death. Only called on this object's input authority.
    /// </summary>
    public void ClientDeath()
    {
        CameraBobbing cb = FindFirstObjectByType<CameraBobbing>();
        cb.isBobbing = false;
        cb.isSprinting = false;
        FindAnyObjectByType<PlayerManager>().onDestroyPlayer?.Invoke();
    }

    private void RegenStamina()
    {
        if (stamina == maxStamina) return;
        if (stamina <= maxStamina)
        {
            stamina += staminaRegenSpeed * Runner.DeltaTime;
            if (stamina > maxStamina) stamina = maxStamina;
        }
    }

    /// <summary>
    /// Returns true and consumes stamina instantly if stamina is able to be consumed
    /// </summary>
    /// <param name="amount">Amount of stamina to be consumed instantly</param>
    /// <returns></returns>
    public bool ConsumeStamina(float amount)
    {
        if (stamina - amount <= 0f) return false;
        if (staminaCooldown > 0f) return false;
        stamina -= amount;

        return true;
    }

    /// <summary>
    /// Returns true and consumes stamina at a rate if stamina is able to be consumed
    /// </summary>
    /// <param name="rate">Rate of stamina to be consumed</param>
    /// <returns></returns>
    public bool RateConsumeStamina(float rate)
    {
        if (stamina <= 0f) return false;
        if (staminaCooldown > 0f) return false;
        stamina -= rate * Runner.DeltaTime;
        
        return true;
    }
}

