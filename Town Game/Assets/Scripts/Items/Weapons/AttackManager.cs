using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
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
        cShake = FindObjectOfType<CameraShake>();
        fps = FindObjectOfType<FirstPerson>();
        camTransform = Camera.main.transform;
        //if (!view.IsMine) return;
        foreach(Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private void Update()
    {
        //if (!view.IsMine) return;
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
        isAttacking = false;
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
        string cState = weapon.clientAnimations[currentAtk];
        animator.Play(state.animation, animator.GetLayerIndex(state.layer), 0f);
        fps.PlayItemUseAnimation(cState);
        //view.RPC("AttackManagerPlay", RpcTarget.Others, state.animation, animator.GetLayerIndex(state.layer));
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
        //Debug.Break();
        atkCooldown = weapon.attackCooldown;
        yield return new WaitForSeconds(animLength - weapon.attackCharge);
        tWeight = 0f;
        isAttacking = false;
        animTimer = animationCooldown;
    }

    public void PlayShake(Shake shake)
    {
        cShake.StartShake(shake.shakeProperties);
    }

    public void CastAttackRay(float distance, float damage, Weapon weapon = null)
    {
        //RaycastHit hit;
        //if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, distance, dmgMask))
        //{
        //    PhotonView view = hit.transform.GetComponent<PhotonView>();
        //    if (view == null)
        //    {
        //        Debug.LogError("View not found!");
        //        return;
        //    }
        //    if (weapon != null)
        //    {
        //        view.RPC("AddEvidence", view.Owner, "cause", weapon.evidenceIcons, weapon.evidenceDescriptions, 0f);
        //        SoundManager.instance.Play3D(weapon.damageSounds[Random.Range(0, weapon.damageSounds.Length)], hit.transform.position);
        //    }
        //    view.RPC("Damage", view.Owner, damage, true);
        //    return;
        //}
        //RaycastHit hit2;
        //if (Physics.Raycast(camTransform.position, camTransform.forward, out hit2, distance, (int)environmentMask))
        //{
        //    SoundMaterial hitSM = hit2.transform.GetComponent<SoundMaterial>();
        //    Texture2D hitTex = hitSM.hitTexture;
        //    Vector2 uv = hit2.textureCoord;
        //    uv.x *= hitTex.width;
        //    uv.y *= hitTex.height;
        //    Color color = hitTex.GetPixel(Mathf.RoundToInt(uv.x), Mathf.RoundToInt(uv.y));
        //    Vector3 rotation = Quaternion.FromToRotation(Vector3.up, hit2.normal).eulerAngles;
        //    Vector3 tColor = new Vector3(color.r, color.g, color.b);
        //    ParticleManager.instance.SpawnParticle("Chunks", hit2.point, rotation, tColor);
        //    ParticleManager.instance.transform.GetComponent<PhotonView>().RPC("SpawnParticle", RpcTarget.Others, "Chunks", hit2.point, rotation, tColor);

        //    SoundMaterial sma = hit2.transform.GetComponent<SoundMaterial>();
        //    if (sma == null) return;
        //    string mat = sma.GetSMat(hit2.textureCoord);
        //    SoundManager.instance.Play3D(mat + "Hit" + Random.Range(0, 3).ToString(), hit2.point);
        //}
    }

    //PhotonView GetDamageView(Transform checkedTransform)
    //{
    //    while (checkedTransform.parent != null)
    //    {
    //        checkedTransform = checkedTransform.parent;
    //        if (checkedTransform.gameObject.tag == "Player")
    //        {
    //            return checkedTransform.GetComponent<PhotonView>();
    //        }
    //    }
    //    return null;
    //}

    //[PunRPC]
    public void AttackManagerPlay(string animation, int index)
    {
        animator.Play(animation, index, 0f);
    }
}
