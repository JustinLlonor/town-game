using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ClientGFX : MonoBehaviour
{
    public LayerMask uiFrontMask;
    public LayerMask clientGFXMask;
    //public PhotonView view;
    public GameObject[] renderers;
    public MeshRenderer serverItem;
    public NetworkObject no;
    public Player playerScript;

    private void Awake()
    {
        playerScript.Init += SetupRenderers;
    }

    private void SetupRenderers()
    {
        // Add client check here
        if (!no.HasInputAuthority) return;
        HideRenderers();
        serverItem.gameObject.layer = (int)Mathf.Log(clientGFXMask.value, 2f);
        //serverItem.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }

    public void ShowRenderers()
    {
        Debug.Log("Showing renderers");
        foreach (GameObject go in renderers)
        {
            go.layer = (int)Mathf.Log(uiFrontMask.value, 2f);
            /**
            if (go.GetComponent<SkinnedMeshRenderer>() != null)
            {
                go.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
            }
            if (go.GetComponent<MeshRenderer>() != null)
            {
                go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
            }
            **/
        }
    }

    public void HideRenderers()
    {
        Debug.Log("hiding renderers");
        foreach(GameObject go in renderers)
        {
            go.layer = (int)Mathf.Log(clientGFXMask.value, 2f);
            /**
            if (go.GetComponent<SkinnedMeshRenderer>() != null)
            {
                go.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
            if (go.GetComponent<MeshRenderer>() != null)
            {
                go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
            **/
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
