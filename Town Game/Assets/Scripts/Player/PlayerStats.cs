using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("HP")]
    [SerializeField] float maxHP = 100f;
    [SerializeField] float HP = 100f;
    [SerializeField] float HPRegenSpeed = 5f;
    [Header("Stamina")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float stamina = 100f;
    [SerializeField] float staminaRegenSpeed = 20f;
    [SerializeField] float regenCooldownPoint = 1f;
    public float staminaCooldown = 0f;
    public bool canRegenStamina = true;
    [Header("Death")]
    public GameObject corpsePrefab;
    public Transform myRig;
    [Header("Hurt")]
    public string hurtLayer = "Hurt";
    public float hurtWeight = 0.4f;
    public float hurtLerp = 50f;
    public Shake softHurt;
    public Shake hardHurt;
    public float softThreshold = 30f;

    public OnDeathEvent OnDeath;

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
        if (Input.GetKeyDown(KeyCode.B))
        {
            PhotonNetwork.LeaveRoom();
        }
        FixDmg();
        if (!view.IsMine) return;
        if (Input.GetKey(KeyCode.P))
        {
            Kill();
        }
        if (staminaCooldown > 0f)
        {
            staminaCooldown -= Time.deltaTime;
        }
        if (staminaCooldown <= regenCooldownPoint && canRegenStamina && pm.isGrounded)
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
        OnDeath.Invoke(corpse);
        pe.ApplyEvidence(corpse);
        Ragdoller ragdoller = corpse.GetComponent<Ragdoller>();
        ragdoller.SetPositionsToTarget(myRig);
        FindObjectOfType<CameraBobbing>().isBobbing = false;

        //corpse.GetComponent<PhotonView>().TransferOwnership(0);
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
        
    }
}

[System.Serializable]
public class OnDeathEvent : UnityEvent<GameObject>
{
}
