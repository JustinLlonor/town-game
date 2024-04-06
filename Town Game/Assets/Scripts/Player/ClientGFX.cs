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
    
    private void Awake()
    {
        if (view.IsMine)
        {
            HideRenderers();
            serverItem.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
    }

    public void ShowRenderers()
    {
        foreach (GameObject go in renderers)
        {
            if (go.GetComponent<SkinnedMeshRenderer>() != null)
            {
                go.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
            }
            if (go.GetComponent<MeshRenderer>() != null)
            {
                go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
            }
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

    public void SetRenderersLayer(LayerMask layer)
    {
        foreach (GameObject go in renderers)
        {
            go.layer = (int)Mathf.Log((float)layer, 2);
        }
    }

}
