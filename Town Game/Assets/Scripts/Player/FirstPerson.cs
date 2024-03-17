using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPerson : MonoBehaviour
{
    public PlayerMovement trackedMV;
    public Transform itemTransform;
    public SkinnedMeshRenderer armsRenderer;
    MeshFilter itemFilter;
    MeshRenderer itemRenderer;
    Animator animator;
    Item currentItem;

    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        trackedMV.OnLeap += OnLeap;
        itemFilter = itemTransform.GetComponent<MeshFilter>();
        itemRenderer = itemTransform.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        animator.SetBool("isRunning", trackedMV.isSprinting);
        animator.SetBool("isGrounded", trackedMV.isGrounded);
    }

    void OnLeap()
    {
        animator.Play("Jump_f");
    }

    public void ShowClientItem(Item item)
    {
        StopAllCoroutines();
        itemFilter.mesh = item.mesh;
        itemRenderer.material.SetTexture("_MainTex", item.texture);
        int gripIndex = animator.GetLayerIndex("Grip");
        int itemIndex = animator.GetLayerIndex("Item");
        animator.Play(item.gripPose);
        animator.Play(item.holdPose);
        animator.SetLayerWeight(gripIndex, 1f);
        animator.SetLayerWeight(itemIndex, 1f);
        currentItem = item;
    }

    public void HideClientItem()
    {
        StopAllCoroutines();
        itemFilter.mesh = null;
        int gripIndex = animator.GetLayerIndex("Grip");
        int itemIndex = animator.GetLayerIndex("Item");
        animator.SetLayerWeight(gripIndex, 0f);
        animator.SetLayerWeight(itemIndex, 0f);
        currentItem = null;
    }
    
    public void PlayItemUseAnimation(string animation)
    {
        StopAllCoroutines();
        int gripIndex = animator.GetLayerIndex("Grip");
        int itemIndex = animator.GetLayerIndex("Item");
        animator.SetLayerWeight(gripIndex, 0f);
        animator.Play(animation, itemIndex, 0f);

        StartCoroutine(WaitForAnimation());
    }

    IEnumerator WaitForAnimation()
    {
        yield return null;
        int itemIndex = animator.GetLayerIndex("Item");
        float seconds = animator.GetCurrentAnimatorStateInfo(itemIndex).length;
        yield return new WaitForSeconds(seconds + 0.1f);
        int gripIndex = animator.GetLayerIndex("Grip");
        animator.SetLayerWeight(gripIndex, 1f);
        animator.CrossFade(currentItem.holdPose, 0.5f);
    }

    public void ChangeArmMesh(Mesh mesh)
    {
        armsRenderer.sharedMesh = mesh;
    }
}
