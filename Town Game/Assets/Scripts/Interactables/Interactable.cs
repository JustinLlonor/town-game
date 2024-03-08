using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public bool canInteract = true;
    public Hover[] hovers;
    [Header("Glow")]
    public bool glow = false;
    public Renderer[] renderers;
    public float glowAmount = 1f;

    bool isGlowing = false;

    [Serializable]
    public class Hover
    {
        public string lore;
        public InteractKey interactKey;
        public Color color = Color.white;
        public float delay;
        public UnityEvent action;
    }

    private void OnDestroy()
    {
        InteractableFinder ifi = FindObjectOfType<InteractableFinder>();
        if (ifi == null) return;
        if (ifi.currentInteraction == this)
        {
            ifi.ResetInteractions();
        }
    }

    public enum InteractKey
    {
        None = 0,
        Interact1 = 1,
        Interact2 = 2,
        Interact3 = 3,
    }

    public void GlowMaterials()
    {
        if (!glow) return;
        foreach (Renderer r in renderers)
        {
            r.material.SetFloat("_RimBrightness", glowAmount);
        }
        isGlowing = true;
    }

    public void UnglowMaterials()
    {
        if (!isGlowing) return;
        foreach (Renderer r in renderers)
        {
            r.material.SetFloat("_RimBrightness", .3f);
        }
        isGlowing = false;
    }
}
