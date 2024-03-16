using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPerson : MonoBehaviour
{
    public PlayerMovement trackedMV;
    Animator animator;

    private void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
        trackedMV.OnLeap += OnLeap;
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
}
