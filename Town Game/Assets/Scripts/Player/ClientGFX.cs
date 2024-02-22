using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ClientGFX : MonoBehaviour
{
    public PhotonView view;
    public GameObject[] renderers;
    public MeshRenderer serverItem;
    
    private void Start()
    {
        Debug.Log("hi");
        if (view.IsMine)
        {
            HideRenderers();
            serverItem.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
    }

    public void HideRenderers()
    {
        foreach(GameObject go in renderers)
        {
            if (go.GetComponent<SkinnedMeshRenderer>() != null)
            {
                go.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
            if (go.GetComponent<MeshRenderer>() != null)
            {
                go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }
    }
}
