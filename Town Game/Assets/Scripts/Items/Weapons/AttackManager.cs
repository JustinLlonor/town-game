using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AttackManager : MonoBehaviour
{
    public bool isAttacking = false;
    public float weightLerp = 50f;
    [HideInInspector] public LayerMask playerMask;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Animator armsAnimator;
    [HideInInspector] public ArmsManager armsManager;
    [HideInInspector] public PhotonView view;

    float tWeight;
    float currentWeight;
    Transform camTransform;
    float atkCooldown;
    float yOffset;
    int currentAtk = 0;
    List<string> resetLayers = new List<string>();

    private void Awake()
    {
        camTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (!view.IsMine) return;
        //armsManager.traceItem = !isAttacking;
        if (isAttacking)
        {
            //armsManager.FollowCam(yOffset);
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
                //armsAnimator.SetLayerWeight(animator.GetLayerIndex(layer), currentWeight);
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
        isAttacking = false;
        currentAtk = 0;
        ResetAnimations();
    }

    public void ResetAnimations()
    {
        foreach (string layer in resetLayers)
        {
            currentWeight = 0f;
            tWeight = 0f;
            animator.SetLayerWeight(animator.GetLayerIndex(layer), 0f);
            animator.Play("New State", animator.GetLayerIndex(layer));
            //armsAnimator.SetLayerWeight(animator.GetLayerIndex(layer), 0f);
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
        view.RPC("AttackManagerPlay", RpcTarget.All, state.animation, animator.GetLayerIndex(state.layer));
        tWeight = 1f;
        //armsAnimator.Play(state.animation, armsAnimator.GetLayerIndex(state.layer));
        //armsAnimator.SetLayerWeight(animator.GetLayerIndex(state.layer), 1f);
        if (!resetLayers.Contains(state.layer)) resetLayers.Add(state.layer);
        StartCoroutine(Charge(weapon, animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex(state.layer)).length));
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
        yield return new WaitForSeconds(weapon.attackCharge);
        CastAttackRay(weapon.range, weapon.damage);
        atkCooldown = weapon.attackCooldown;
        yield return new WaitForSeconds(animLength - weapon.attackCharge);
        tWeight = 0f;
        isAttacking = false;
    }

    public void CastAttackRay(float distance, float damage)
    {
        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, distance, (int)playerMask))
        {
            PhotonView view = hit.transform.GetComponent<PhotonView>();
            view.RPC("Damage", view.Owner, damage);
        }
    }

    [PunRPC]
    public void AttackManagerPlay(string animation, int index)
    {
        animator.Play(animation, index);
    }
}
