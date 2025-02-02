using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Fusion;

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
        public Action[] actions;
        public Color color = Color.white;
        public float delay;
        public bool trackLore = false;
        public bool trackColor = false;
        public NetworkSettings networkSettings;

        [Serializable]
        public class NetworkSettings
        {
            public bool networked = false;
            public UnityEvent clientAction;
        }
    }

    public enum InteractKey
    {
        None = 0,
        Interact1 = 1,
        Interact2 = 2,
        Interact3 = 3,
    }

    [Serializable]
    public struct Action
    {
        public GameObject actionObject;
        public string methodName;
        public bool passPlayerRef;

        public void Invoke(PlayerRef player = default)
        {
            if (passPlayerRef)
            {
                actionObject.SendMessage(methodName, player);
                return;
            }
            actionObject.SendMessage(methodName);
        }
    }

    private void OnDestroy()
    {
        InteractableFinder ifi = FindObjectOfType<InteractableFinder>();
        if (ifi == null) return;
        if (ifi.currentInteraction == this)
        {
            CrosshairManager.instance.RemoveCrosshair(0);
            ifi.iui.ClearInteractions();
        }
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
