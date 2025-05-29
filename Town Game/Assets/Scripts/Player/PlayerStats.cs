using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [Header("HP")]
    public int maxHP = 3;
    [Networked] public int HP { get; set; } = 3;
    private int previousHP = 3;
    [Header("Hunger")]
    public int maxHunger = 3;
    [Networked] public int hunger { get; set; } = 3;
    [Tooltip("The amount of periods in between each hunger tick")]
    public float hungerTickDelay = 6f;
    private int previousHunger = 3;
    /// <summary>
    /// The next period for the hunger check to occur
    /// </summary>
    private float hungerTickPeriod = 0f;
    [Header("Stamina")]
    public float maxStamina = 100f;
    [Networked] public float stamina { get; set; } = 100f;
    [SerializeField] float staminaRegenSpeed = 20f;
    [SerializeField] float regenCooldownPoint = 1f;
    [Networked] public float staminaCooldown { get; set; }
    public float staminaRegenCooldown = .5f;
    public bool canRegenStamina = true;
    [Header("Odour")]
    public float maxOdourLevel = 100f;
    [Networked] public float odourLevel { get; set; } = 0f;
    /// <summary>
    /// The strongest odour present on this player
    /// </summary>
    [Networked] public NetworkString<_16> prominentOdour { get; set; }
    public List<OdourPresence> odourPrescences = new List<OdourPresence>();
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

    [System.Serializable]
    public class OdourPresence
    {
        public string odour;
        /// <summary>
        /// The amount of odour present
        /// </summary>
        public float amount;

        public OdourPresence(string odour, float amount)
        {
            this.odour = odour;
            this.amount = amount;
        }
    }

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
        previousHunger = hunger;
    }

    public override void Render()
    {
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
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority) ClientDeath();
    }

    public override void FixedUpdateNetwork()
    {
        piv.recievedHP = HP;
        if (staminaCooldown > 0f) staminaCooldown -= Runner.DeltaTime;
        if (staminaRegenCooldown > 0f) staminaRegenCooldown -= Runner.DeltaTime;
        if (staminaCooldown <= regenCooldownPoint && canRegenStamina && pm.isGrounded && staminaRegenCooldown <= 0f)
        {
            RegenStamina();
        }
        HungerCheck();
    }

    // Health
    #region
    /// <summary>
    /// Sets the health of this player
    /// </summary>
    /// <param name="health"></param>
    public void SetHealth(int health)
    {
        int previousHealth = HP;
        HP = health;
        onHPChange?.Invoke(HP - previousHealth);
    }

    /// <summary>
    /// Damages the player by the specified amount
    /// </summary>
    /// <param name="amount"></param>
    public void Damage(int amount)
    {
        if (!Runner.IsServer) return;
        HP -= amount;
        onHPChange?.Invoke(-amount);
        CheckDeath();
        /**
        if (playShake)
        {
            if (amount < softThreshold)
            {
                shake.StartShake(softHurt.shakeProperties);
            }
            else
            {
                shake.StartShake(hardHurt.shakeProperties);
            }
        }
        **/
    }

    /// <summary>
    /// Heals the player by the specified amount
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(int amount)
    {
        HP += amount;
        if (HP > maxHP) HP = maxHP;
        onHPChange?.Invoke(amount);
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
        if (HP <= 0)
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
    #endregion

    // Hunger
    #region
    /// <summary>
    /// Sets the hunger of this player
    /// </summary>
    /// <param name="amount"></param>
    public void SetHunger(int amount)
    {
        amount = Mathf.Clamp(amount, 0, maxHunger);
        int previousHunger = hunger;
        hunger = amount;
        onHungerChange?.Invoke(hunger - previousHunger);
    }

    /// <summary>
    /// Adds hunger by the specified amount
    /// </summary>
    /// <param name="amount"></param>
    public void AddHunger(int amount)
    {
        SetHunger(hunger + amount);
    }

    /// <summary>
    /// Removes hunger by the specified amount
    /// </summary>
    /// <param name="amount"></param>
    public void RemoveHunger(int amount)
    {
        SetHunger(hunger - amount);
    }

    /// <summary>
    /// To be called at specified periods of the day. 
    /// Removes 1 hunger from the player. If the player's hunger is already 0, then removes 1 HP from the player
    /// </summary>
    public void TickHunger()
    {
        if (hunger <= 0)
        {
            Damage(1);
            return;
        }
        RemoveHunger(1);
    }

    private void HungerCheck()
    {
        if (!HasStateAuthority) return;
        if (gameManager == null) return;
        if (gameManager.currentPeriod > hungerTickPeriod)
        {
            hungerTickPeriod += hungerTickDelay;
            TickHunger();
        }
    }
    #endregion

    // Stamina
    #region
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
    #endregion

    // Odour
    #region
    /// <summary>
    /// Adds the specified odour to the player
    /// </summary>
    /// <param name="odour">The name of the odour</param>
    /// <param name="amount">The amount of odour to add</param>
    public void AddOdour(string odour, float amount)
    {
        if (!HasStateAuthority) return;
        if (!objectManager.odourSearch.ContainsKey(odour))
        {
            Debug.LogError("Odour " + odour + " does not exist.");
            return;
        }

        // Add the odour to the list if its not there. add the total amount to the added prescence
        OdourPresence addedPresence = GetOdourPrescence(odour);
        if (addedPresence == null)
        {
            addedPresence = new OdourPresence(odour, 0f);
            odourPrescences.Add(addedPresence);
        }
        addedPresence.amount += amount;

        // Decrease the total odour from every other odour if it exceeds the max odour level
        float newLevel = GetTotalOdourLevel();
        if (newLevel > maxOdourLevel)
        {
            float totalRemoval = newLevel - maxOdourLevel;
            DecreaseTotalOdour(totalRemoval, odour);
        }

        odourLevel = GetTotalOdourLevel();
        prominentOdour = GetProminentOdour();
    }

    /// <summary>
    /// Removes the prescence of every odour by the specified amount
    /// </summary>
    /// <param name="amount"></param>
    public void RemoveOdour(float amount)
    {
        if (!HasStateAuthority) return;
        DecreaseTotalOdour(amount);
        odourLevel = GetTotalOdourLevel();
        if (odourLevel <= 0f) prominentOdour = "";
    }

    /// <summary>
    /// Decreases the specified amount from every odour in total, excluding the excluded odour
    /// </summary>
    /// <param name="totalRemoval"></param>
    /// <param name="excludedOdour"></param>
    private void DecreaseTotalOdour(float totalRemoval, string excludedOdour)
    {
        if (odourPrescences.Count == 1)
        {
            odourPrescences[0].amount = maxOdourLevel;
            return;
        }
        if (odourPrescences.Count == 0)
        {
            Debug.LogError("No odours to remove from.");
            return;
        }
        float decreasedAmount = totalRemoval / (odourPrescences.Count - 1);
        foreach (OdourPresence prescence in odourPrescences)
        {
            if (prescence.odour == excludedOdour) continue;
            prescence.amount -= decreasedAmount;
        }
        // Deleting a prescence will also delete its negative amount.
        // This will re-add this negative amount to the excluded odour to get the cap back to the max odour level
        float leftovers = CheckOdourRemoval();
        if (leftovers < 0f)
        {
            GetOdourPrescence(excludedOdour).amount += leftovers;
        }
    }

    /// <summary>
    /// Decreases the total odour by the specified amount
    /// </summary>
    /// <param name="amount"></param>
    private void DecreaseTotalOdour(float amount)
    {
        if (amount == 0f) return;
        if (odourPrescences.Count == 0) return;
        float decreasedAmount = amount / odourPrescences.Count;
        foreach (OdourPresence prescence in odourPrescences)
        {
            prescence.amount -= decreasedAmount;
        }
        float leftovers = CheckOdourRemoval();
        DecreaseTotalOdour(-leftovers);
    }

    /// <summary>
    /// Removes all odour prescences less than 0
    /// </summary>
    /// <returns>The amount leftover from the odour removal</returns>
    private float CheckOdourRemoval()
    {
        float output = 0f;
        for (int i = 0; i < odourPrescences.Count; i++)
        {
            if (odourPrescences[i].amount <= 0f)
            {
                output += odourPrescences[i].amount;
                odourPrescences.RemoveAt(i);
                i--;
            }
        }
        return output;
    }

    private OdourPresence GetOdourPrescence(string odour)
    {
        foreach (OdourPresence o in odourPrescences)
        {
            if (o.odour == odour) return o;
        }
        return null;
    }

    private float GetTotalOdourLevel()
    {
        float output = 0f;
        foreach (OdourPresence o in odourPrescences)
        {
            output += o.amount;
        }
        return output;
    }

    private string GetProminentOdour()
    {
        float highestOdour = 0f;
        string output = "";
        foreach (OdourPresence o in odourPrescences)
        {
            if (o.amount > highestOdour)
            {
                output = o.odour;
                highestOdour = o.amount;
            }
        }
        return output;
    }
    #endregion
}

