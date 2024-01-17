using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public bool canInteract = true;
    public Hover[] hovers;
    [Header("Glow")]
    public bool glow = false;
    public Renderer[] renderers;
    public float glowAmount = .5f;

    bool isGlowing = false;

    public void GlowMaterials()
    {
        if (!glow) return;
        foreach (Renderer r in renderers)
        {
            r.material.SetFloat("_Power", glowAmount);
        }
        isGlowing = true;
    }

    public void UnglowMaterials()
    {
        if (!isGlowing) return;
        foreach (Renderer r in renderers)
        {
            r.material.SetFloat("_Power", 10000f);
        }
        isGlowing = false;
    }

    [System.Serializable]
    public struct Hover
    {
        public string lore;
        public KeyCode key;
        public UnityEvent action;
    }
}
