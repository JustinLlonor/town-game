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
    public LayerMask dmgMask;
    public LayerMask environmentMask;
    [HideInInspector] public Animator animator;
    //[HideInInspector] public PhotonView view;
    [HideInInspector] public Transform animHolder;

    FirstPerson fps;
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
        cShake = FindFirstObjectByType<CameraShake>();
        fps = FindFirstObjectByType<FirstPerson>();
        camTransform = Camera.main.transform;
        //if (!view.IsMine) return;
        foreach(Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private void Update()
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
