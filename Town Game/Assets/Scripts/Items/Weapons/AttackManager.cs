using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AttackManager : MonoBehaviour
{
    public bool isAttacking = false;
    public float animationCooldown = .1f;
    public float weightLerp = 50f;
    [HideInInspector] public LayerMask playerMask;
    [HideInInspector] public Animator animator;
    [HideInInspector] public PhotonView view;
    [HideInInspector] public Animator itemAnimator;
    [HideInInspector] public Transform animHolder;

    CameraShake cShake;
    float tWeight;
    float currentWeight;
    float atkCooldown;
    float animTimer;
    Transform camTransform;
    int currentAtk = 0;
    List<string> resetLayers = new List<string>(); 

    private void Awake()
    {
        cShake = FindObjectOfType<CameraShake>();
        camTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (!view.IsMine) return;
        if (animTimer  >= 0 && !isAttacking)
        {
            animTimer -= Time.deltaTime;
            if (animTimer < 0f)
            {
                currentAtk = 0;
            }
        }
        if (atkCooldown > 0f)
        {
            atkCooldown -= Time.deltaTime;
        }
        if (currentWeight != tWeight)
        {
            currentWeight = Mathf.Lerp(currentWeight, tWeight, Time.deltaTime * weightLerp);
            foreach (string layer in resetLayers)
            {
                animator.SetLayerWeight(animator.GetLayerIndex(layer), currentWeight);
            }
            if (Mathf.Abs(currentWeight - tWeight) < 0.01f)
            {
                currentWeight = tWeight;
                if (currentWeight == 0f)
                {
                    resetLayers.Clear();
                }
            }
        }
    }

    public void ResetAttack()
    {
        StopAllCoroutines();
        itemAnimator.enabled = false;
        isAttacking = false;
        animHolder.localRotation = Quaternion.identity;
        animTimer = -.0001f;
        currentAtk = 0;
        ResetAnimations();
    }

    public void ResetAnimations()
    {
        currentWeight = 0f;
        tWeight = 0f;
        foreach (string layer in resetLayers)
        {
            animator.SetLayerWeight(animator.GetLayerIndex(layer), 0f);
            animator.Play("New State", animator.GetLayerIndex(layer));
            itemAnimator.Play("New State", 0);
        }
        resetLayers.Clear();
    }

    public void SetAttackCooldown(float cooldown)
    {
        atkCooldown = cooldown;
    }

    public void Attack(Weapon weapon)
    {
        if (atkCooldown > 0) return;
        if (isAttacking) return;

        StopAllCoroutines();
        if (currentAtk == weapon.useAnimations.Length) currentAtk = 0;
        Item.AnimationState state = weapon.useAnimations[currentAtk];
        Item.AnimationState cState = weapon.clientAnimations[currentAtk];
        animator.Play(state.animation, animator.GetLayerIndex(state.layer));
        itemAnimator.enabled = true;
        itemAnimator.Rebind();
        itemAnimator.Update(0f);
        itemAnimator.Play(cState.animation);
        view.RPC("AttackManagerPlay", RpcTarget.Others, state.animation, animator.GetLayerIndex(state.layer));
        tWeight = 1f;
        if (!resetLayers.Contains(state.layer)) resetLayers.Add(state.layer);
        StartCoroutine(Charge(weapon, weapon.attackLength));
        currentAtk++;
    }

    IEnumerator Charge(Weapon weapon, float animLength)
    {
        isAttacking = true;
        if (weapon.attackCharge > animLength)
        {
            Debug.LogError("Charge is longer than animation!");
            yield break;
        }
        SoundManager.instance.Play3D(weapon.attackSounds[Random.Range(0, weapon.attackSounds.Length)], transform.position);
        yield return new WaitForSeconds(weapon.attackCharge);
        PlayShake(weapon.shake);
        CastAttackRay(weapon.range, weapon.damage, weapon);
        atkCooldown = weapon.attackCooldown;
        yield return new WaitForSeconds(animLength - weapon.attackCharge);
        tWeight = 0f;
        isAttacking = false;
        itemAnimator.enabled = false;
        animTimer = animationCooldown;
        animHolder.localRotation = Quaternion.identity;
    }

    public void PlayShake(Shake shake)
    {
        cShake.StartShake(shake.shakeProperties);
    }

    public void CastAttackRay(float distance, float damage, Weapon weapon = null)
    {
        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, distance, (int)playerMask))
        {
            Transform tTransform = hit.transform;
            PhotonView view = tTransform.GetComponent<PhotonView>();
            if (weapon != null)
            {
                view.RPC("AddEvidence", view.Owner, "cause", weapon.evidenceIcons, weapon.evidenceDescriptions, 0f);
                SoundManager.instance.Play3D(weapon.damageSounds[Random.Range(0, weapon.damageSounds.Length)], hit.transform.position);
            }
            view.RPC("Damage", view.Owner, damage);
        }
    }

    [PunRPC]
    public void AttackManagerPlay(string animation, int index)
    {
        animator.Play(animation, index);
    }
}
