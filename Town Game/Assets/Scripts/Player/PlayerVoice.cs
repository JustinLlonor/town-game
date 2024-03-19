using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice;
using Photon.Voice.Unity;

public class PlayerVoice : MonoBehaviour
{
    public KeyCode key;
    public Animator animator;

    Recorder rec;

    private void Awake()
    {
        rec = FindObjectOfType<Recorder>();
        rec.RecordingEnabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            animator.SetBool("isTalking", true);
            rec.RecordingEnabled = true;
        }
        if (Input.GetKeyUp(key))
        {
            animator.SetBool("isTalking", false);
            rec.RecordingEnabled = false;
        }
    }
}
