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
    [Tooltip("If this item glows when hovered on")]
    public bool glow = false;
    public Renderer[] renderers;
    public float glowAmount = 1f;
    
    /// <summary>
    /// Called when the player looks at this interactable on the client
    /// </summary>
    public InteractableEvent onLook;
    /// <summary>
    /// Called when the player looks away from this interactable on the client
    /// </summary>
    public InteractableEvent onLookAway;

    public delegate void InteractableEvent();

    bool isGlowing = false;
    /// <summary>
    /// If the player is looking at this interactable
    /// </summary>
    public bool isLooking { get; private set; } = false;

    [Serializable]
    public class Hover
    {
        public string lore;
        public InteractKey interactKey;
        public Action[] actions;
        public Color color = Color.white;
        public Color fillColor = new Color(0, 0, 0, 0.5607843f);
        public Color keyColor = Color.black;
        public float delay;
        public bool trackLore = false;
        public bool trackColor = false;
        public NetworkSettings networkSettings;

        public Hover()
        {
            color = Color.white;
            fillColor = new Color(0, 0, 0, 0.5607843f);
            keyColor = Color.black;
        }

        [Serializable]
        public class NetworkSettings
        {
            public bool networked = false;
            public UnityEvent clientAction;
            [Tooltip("If this is set to a key, then this hover action will trigger always with this key, even if the key is different on the server.")]
            public InteractKey indiffKey = InteractKey.None;
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
        InteractableFinder ifi = FindFirstObjectByType<InteractableFinder>();
        if (ifi == null) return;
        if (ifi.currentInteraction == this)
        {
            CrosshairManager.instance.RemoveCrosshair(0);
            ifi.iui.ClearInteractions();
        }
    }

    /// <summary>
    /// When player looks at this interactable, called on the client
    /// </summary>
    public void Look()
    {
        GlowMaterials();
        InvokeLookEvent();
    }

    public void Unlook()
    {
        UnglowMaterials();
        InvokeLookAwayEvent();
    }

    private void InvokeLookEvent()
    {
        if (isLooking) return;
        isLooking = true;
        onLook?.Invoke();
    }

    private void InvokeLookAwayEvent()
    {
        if (!isLooking) return;
        isLooking = false;
        onLookAway?.Invoke();
    }

    private void GlowMaterials()
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

    [ContextMenu("Add Default Hover")]
    public void AddDefaultHover()
    {
        hovers = new Hover[] { new Hover() };
    }
}
