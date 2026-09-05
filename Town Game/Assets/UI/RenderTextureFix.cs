using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderTextureFix : MonoBehaviour
{
    public RenderTexture rt;
    public RawImage ri;
    Camera uiFront;
    
    private void Awake()
    {
        uiFront = FindFirstObjectByType<CameraManager>().uiFront;
        uiFront.targetTexture = null;
        rt.Release();
        rt.height = Screen.height;
        rt.width = Screen.width;
        rt.Create();
        uiFront.targetTexture = rt;
    }

    private void Start()
    {
        ri.enabled = true;
        ri.texture = rt;
    }
}
