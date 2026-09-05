using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine.InputSystem;
using Fusion;

public class PlayerVoice : NetworkBehaviour
{
    public Animator animator;
    Recorder rec;
    InputManager inputManager;
    public Transform speakerT;
    public Speaker speaker;

    private void OnDestroy()
    {
        if (rec != null) rec.TransmitEnabled = false;
    }

    public override void Spawned()
    {
        if (!HasInputAuthority) return;
        inputManager = FindAnyObjectByType<InputManager>();
        inputManager.onVoice += ToggleVoice;
        rec = Runner.gameObject.GetComponent<Recorder>();
        rec.TransmitEnabled = false;
    }

    private void ToggleVoice(InputValue value)
    {
        bool activated = value.Get<float>() == 1f;
        rec.TransmitEnabled = activated;
    }

    private void Update()
    {
        if (speaker == null)
        {
            speaker = speakerT.GetComponent<Speaker>();
        }
        animator.SetBool("isTalking", speaker.IsPlaying);
    }


}
