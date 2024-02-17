using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    public Transform footstepRaycast;
    public LayerMask environmentMask;

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
        RaycastHit hit;
        if (Physics.Raycast(footstepRaycast.position, footstepRaycast.up * -1f, out hit, 1f, (int)environmentMask))
        {
            SoundMaterial sma = hit.transform.GetComponent<SoundMaterial>();
            if (sma == null) return;
            string mat = sma.GetSMat(hit.textureCoord);
            sm.Play3D(mat + "Step" + Random.Range(0, 3).ToString(), transform.position);
        }
    }
}
