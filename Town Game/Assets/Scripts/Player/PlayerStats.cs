using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
using System.Linq;
using Fusion;
using UnityEngine.SceneManagement;

public class PlayerStats : NetworkBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    [Networked] public float HP { get; set; } = 100f;
    [SerializeField] float HPRegenSpeed = 5f;
    [Header("Nutrition")]
    public float maxNutrition = 100f;
    public float nutrition = 100f;
    [Header("Sanity")]
    public float maxSanity = 100f;
    public float sanity = 100f;
    [Header("Stamina")] // Networking
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
    [Header("Affecter Stuff")]
    public StatAffecter hungerAffecter;
    public List<StatAffecter> affecters = new List<StatAffecter>();

    public delegate void OnDamage(float damage);
    public OnDamage OnTakeDamage;
    public delegate void Death();
    public Death OnDeath;
    public delegate void AffecterChange(StatAffecter affecter);
    public AffecterChange OnRemoveAffecter;
    public AffecterChange OnAddAffecter;

    CameraShake shake;
    //PhotonView view;
    PlayerEvidence pe;
    [HideInInspector] public PlayerMovement pm;
    [HideInInspector] public Animator anim;
    int hLayer;
    float hWeight;

    private void Start()
    {
        //pm = gameObject.GetComponent<PlayerMovement>();
        hLayer = anim.GetLayerIndex(hurtLayer);
        //view = gameObject.GetComponent<PhotonView>();
        pe = gameObject.GetComponent<PlayerEvidence>();
        //if (!view.IsMine) return;
        shake = FindObjectOfType<CameraShake>();
        //if (FindObjectOfType<GameManager>() != null)AddAffector(hungerAffecter);
    }

    private void Update()
    {
        FixDmg(); //Dmg animation
        //if (!view.IsMine) return;
        //CheckAffecters();
    }

    public override void Spawned()
    {
        pm.SetGrounded();
        stamina = 100f;
        staminaCooldown = 0f;
    }

    public override void FixedUpdateNetwork()
    {
        if (staminaCooldown > 0f) staminaCooldown -= Runner.DeltaTime;
        if (staminaRegenCooldown > 0f) staminaRegenCooldown -= Runner.DeltaTime;
        if (staminaCooldown <= regenCooldownPoint && canRegenStamina && pm.isGrounded && staminaRegenCooldown <= 0f)
        {
            RegenStamina();
        }
        if (HP < maxHP)
        {
            RegenHP();
        }
    }

    // Stat affecters
    public void AddAffector(StatAffecter affecter)
    {
        StatAffecter foundAffecter = affecters.FirstOrDefault(i => i.name == affecter.name);
        if (foundAffecter != null) return;
        affecters.Add(new StatAffecter(affecter.name, affecter.description, affecter.stat, affecter.changeRate, affecter.timeLeft, affecter.isInfinite, affecter.display));
        OnAddAffecter?.Invoke(affecter);
    }

    public void RemoveAffecter(string name)
    {
        for (int i = 0; i < affecters.Count; i++)
        {
            if (affecters[i].name == name)
            {
                OnRemoveAffecter?.Invoke(affecters[i]);
                affecters.RemoveAt(i);
                break;
            }
        }
    }

    void CheckAffecters()
    {
        List<StatAffecter> destroyed = new List<StatAffecter>();
        for (int i = 0; i < affecters.Count; i++)
        {
            if (affecters[i].timeLeft <= 0f && !affecters[i].isInfinite)
            {
                destroyed.Add(affecters[i]);
                break;
            }

            StatAffecter affecter = affecters[i];
            float changeAmount = affecter.changeRate * Time.deltaTime;

            switch (affecter.stat)
            {
                // Changes the stat with change amount
                case StatAffecter.Stat.Health:
                    HP = Mathf.Clamp(HP + changeAmount, 0f, maxHP);
                    CheckDeath();
                    break;
                case StatAffecter.Stat.Nutrition:
                    nutrition = Mathf.Clamp(nutrition + changeAmount, 0f, maxNutrition);
                    break;
                case StatAffecter.Stat.Sanity:
                    sanity = Mathf.Clamp(sanity + changeAmount, 0f, maxSanity);
                    break;
            }

            if (!affecter.isInfinite)
            {
                affecter.timeLeft -= Time.deltaTime;
                if (affecter.timeLeft <= 0f)
                {
                    destroyed.Add(affecters[i]);
                }
            }
        }

        foreach (StatAffecter i in destroyed)
        {
            RemoveAffecter(i.name);
        }
    }

    void RegenHP()
    {
        HP += HPRegenSpeed * Runner.DeltaTime;
        if (HP > maxHP)
        {
            HP = maxHP; 
        }
    }

    public void Damage(float amount, bool playShake = true)
    {
        HP -= amount;
        OnTakeDamage?.Invoke(amount);
        CheckDeath();
        //view.RPC("DamageAnimation", RpcTarget.All);
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
    }

    public void DamageAnimation()
    {
        hWeight = hurtWeight;
        anim.SetLayerWeight(hLayer, hWeight);
    }

    void CheckDeath()
    {
        if (HP <= 0f)
        {
            //Kill();
        }
    }

    void FixDmg()
    {
        if (hWeight == 0f) return;
        hWeight = Mathf.Lerp(hWeight, 0f, Time.deltaTime * hurtLerp);
        anim.SetLayerWeight(hLayer, hWeight);
        if (hWeight < 0.001f) hWeight = 0f;
    }

    public void Kill()
    {
        if (HasInputAuthority) ClientDeath();
        if (!HasStateAuthority) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        NetworkObject corpse = Runner.Spawn(corpsePrefab, rb.position, rb.rotation, null, (runner, o) =>
        {
            o.GetComponent<Corpse>().Init(pe, rb.velocity);
        });
        // To initialize: evidence, bone positions, velocity, clothing
        //GameObject corpse = PhotonNetwork.Instantiate(corpsePrefab.name, transform.position, transform.rotation);
        //PhotonView corpseView = corpse.GetComponent<PhotonView>();
        Ragdoller ragdoller = corpse.GetComponent<Ragdoller>();
        ragdoller.SetPositionsToTarget(myRig);
    
        // Set corpse nickname
        //co.SetCorpseData(view.Owner);
        //corpseView.RPC("SetCorpseData", RpcTarget.OthersBuffered, view.Owner);

        // Sets corpse clothing to this player's clothing
        PlayerClothing cpc = corpse.GetComponent<PlayerClothing>();
        PlayerClothing pc = gameObject.GetComponent<PlayerClothing>();
        foreach (PlayerClothing.Attire attire in pc.attires)
        {
            if (attire.clothing == null) continue;
            cpc.SetClothing(attire.clothing.name, cpc.isMale);
            //corpseView.RPC("SetClothing", RpcTarget.OthersBuffered, attire.clothing.name, cpc.isMale);
        }
        //PhotonNetwork.Destroy(gameObject);
        return;

        //Destroy player
    }

    public void ClientDeath()
    {
        OnDeath?.Invoke();
        FindObjectOfType<CameraBobbing>().isBobbing = false;
    }

    // Stamina
    void RegenStamina()
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

    //public override void OnLeftRoom()
    //{
    //    SceneManager.LoadScene(0);
    //}

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        stream.SendNext(HP);
    //        stream.SendNext(maxHP);
    //        stream.SendNext(nutrition);
    //        stream.SendNext(maxNutrition);
    //        stream.SendNext(sanity);
    //        stream.SendNext(maxSanity);
    //    }
    //    else
    //    {
    //        HP = (float)stream.ReceiveNext();
    //        maxHP = (float)stream.ReceiveNext();
    //        nutrition = (float)stream.ReceiveNext();
    //        maxNutrition = (float)stream.ReceiveNext();
    //        sanity = (float)stream.ReceiveNext();
    //        maxSanity = (float)stream.ReceiveNext();
    //    }
    //}
}

