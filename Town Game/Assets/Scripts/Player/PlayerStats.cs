using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviourPunCallbacks, IPunObservable
{
    public float testDmg = 10f;
    [Header("HP")]
    public float maxHP = 100f;
    public float HP = 100f;
    [SerializeField] float HPRegenSpeed = 5f;
    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina = 100f;
    [SerializeField] float staminaRegenSpeed = 20f;
    [SerializeField] float regenCooldownPoint = 1f;
    public float staminaCooldown = 0f;
    public float staminaRegenCooldown = .5f;
    public bool canRegenStamina = true;
    [Header("Death")]
    public GameObject corpsePrefab;
    public Transform myRig;
    [Header("Hurt")]
    public string hurtLayer = "Hurt";
    public float hurtWeight = 0.1f;
    public float hurtLerp = 50f;
    public Shake softHurt;
    public Shake hardHurt;
    public float softThreshold = 30f;

    public delegate void OnDamage(float damage);
    public OnDamage onDamage;

    CameraShake shake;
    PhotonView view;
    PlayerEvidence pe;
    PlayerMovement pm;
    [HideInInspector] public Animator anim;
    int hLayer;
    float hWeight;

    private void Start()
    {
        pm = gameObject.GetComponent<PlayerMovement>();
        hLayer = anim.GetLayerIndex(hurtLayer);
        view = gameObject.GetComponent<PhotonView>();
        pe = gameObject.GetComponent<PlayerEvidence>();
        if (!view.IsMine) return;
        shake = FindObjectOfType<CameraShake>();
    }

    private void Update()
    {
        FixDmg();
        if (!view.IsMine) return;
        if (Input.GetKeyDown(KeyCode.B))
        {
            Damage(testDmg);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Kill();
        }
        if (staminaCooldown > 0f) staminaCooldown -= Time.deltaTime;
        if (staminaRegenCooldown > 0f) staminaRegenCooldown -= Time.deltaTime;
        if (staminaCooldown <= regenCooldownPoint && canRegenStamina && pm.isGrounded && staminaRegenCooldown <= 0f)
        {
            RegenStamina();
        }
        if (HP < maxHP)
        {
            RegenHP();
        }
    }

    // HP
    void RegenHP()
    {
        HP += HPRegenSpeed * Time.deltaTime;
        if (HP > maxHP)
        {
            HP = maxHP; 
        }
    }

    /// <summary>
    /// Instantly removes the specified amount of HP.
    /// </summary>
    /// <param name="amount"></param>
    [PunRPC]
    public void Damage(float amount, bool playShake = true)
    {
        HP -= amount;
        onDamage.Invoke(amount);
        if (HP < 0f)
        {
            Kill();
        }
        view.RPC("DamageAnimation", RpcTarget.All);
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

    [PunRPC]
    public void DamageAnimation()
    {
        hWeight = hurtWeight;
        anim.SetLayerWeight(hLayer, hWeight);
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
        GameObject corpse = PhotonNetwork.Instantiate(corpsePrefab.name, transform.position, transform.rotation);
        PhotonView corpseView = corpse.GetComponent<PhotonView>();
        pe.ApplyEvidence(corpse);
        Ragdoller ragdoller = corpse.GetComponent<Ragdoller>();
        ragdoller.SetPositionsToTarget(myRig);
        FindObjectOfType<CameraBobbing>().isBobbing = false;

        // Set corpse nickname
        Corpse co = corpse.GetComponent<Corpse>();
        co.SetVelocity(gameObject.GetComponent<Rigidbody>().velocity);
        co.SetCorpseData(view.Owner);
        corpseView.RPC("SetCorpseData", RpcTarget.OthersBuffered, view.Owner);

        // Sets corpse clothing to this player's clothing
        PlayerClothing cpc = corpse.GetComponent<PlayerClothing>();
        PlayerClothing pc = gameObject.GetComponent<PlayerClothing>();
        foreach (PlayerClothing.Attire attire in pc.attires)
        {
            if (attire.clothing == null) continue;
            cpc.SetClothing(attire.clothing.name, cpc.isMale);
            corpseView.RPC("SetClothing", RpcTarget.OthersBuffered, attire.clothing.name, cpc.isMale);
        }
        return;

        //Destroy player
        PhotonNetwork.Destroy(gameObject);
    }

    // Stamina
    void RegenStamina()
    {
        if (stamina == maxStamina) return;
        if (stamina <= maxStamina)
        {
            stamina += staminaRegenSpeed * Time.deltaTime;
        }
        else
        {
            stamina = maxStamina;
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
        stamina -= rate * Time.deltaTime;
        
        return true;
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(HP);
            stream.SendNext(maxHP);
        }
        else
        {
            HP = (float)stream.ReceiveNext();
            maxHP = (float)stream.ReceiveNext();
        }
    }
}

