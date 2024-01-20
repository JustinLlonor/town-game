using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    public string[] sounds = new string[] { };

    SoundManager sm;
    PhotonView view;

    private void Awake()
    {
        view = transform.parent.GetComponent<PhotonView>();
        sm = FindObjectOfType<SoundManager>();
    }

    public void PlayFootstep()
    {
        if (!view.IsMine) return;
        sm.Play3D(sounds[Random.Range(0, sounds.Length)], transform.position);
    }
}
