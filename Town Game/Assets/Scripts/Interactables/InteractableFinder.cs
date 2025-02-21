using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class InteractableFinder : NetworkBehaviour
{
    [Header("Masks")]
    public LayerMask interactableMask;
    public LayerMask environmentMask;
    [Header("Settings")]
    public float range = 2f;
    [Header("References")]
    public InteractableUI iui;
    [Header("Keys")]
    public InputActionReference[] interactActions;
    [Networked] public bool canInteract { get; set; } = true;
    bool previousCanInteract = true;

    [HideInInspector] public bool iValid = true;
    public Interactable currentInteraction;
    [HideInInspector] public Player player;
    [HideInInspector] public Transform trackedTransform;
    [HideInInspector] public Rigidbody rb; // To find rb location because only rb location is accurately networked
    GameObject currentInteractable = null;
    RunnerManager rm;
    float timer = 0f;

    [Networked] TickTimer serverTimer { get; set; } // Interaction timer on the server
    [Networked] public bool timerRunning { get; set; } = false; // If the server timer is running
    int heldInteractable = 0;
    [HideInInspector] public bool currentPressed = false; // If the thing is currently pressed
    bool previousPressed = false;
    [HideInInspector] public Interactable.InteractKey currentKey = Interactable.InteractKey.None;

    List<int> trackedIndexes = new List<int>();
    [HideInInspector] public Vector3 forwardDirection;
    [HideInInspector] public bool menuData = false;

    public override void Spawned()
    {
        if (IsProxy) Destroy(this);
        if (HasInputAuthority) UIManager.instance.OnUIOpen += ResetInteractions;
        iui = FindFirstObjectByType<InteractableUI>();
        rm = FindFirstObjectByType<RunnerManager>();
        if (!HasInputAuthority) return;
        InputManager inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onInteract1 += OnInteract1;
        inputManager.onInteract2 += OnInteract2;
        inputManager.onInteract3 += OnInteract3;
    }

    public void SetCanInteract(bool interactability)
    {
        canInteract = interactability;
    }

    private void Update()
    {
        if (canInteract != previousCanInteract)
        {
            previousCanInteract = canInteract;
            if (!canInteract)
            {
                ResetInteractions();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsResimulation)
        {
            if (canInteract)
            {
                if (!menuData && canInteract) CastRay(); // If the menu isn't open for the player, and we can interact, cast a ray
                UpdateTracking(); // Tracked hovers for the client
            }
            if (currentPressed != previousPressed)
            {
                previousPressed = currentPressed;
                OnPressedChange();
            }
        }
        CheckInteractable();
    }

    void CheckInteractable()
    {
        if (timerRunning)
        {
            if (currentInteraction == null) return;
            if (currentInteraction.hovers.Length <= heldInteractable) return;
            if (serverTimer.Expired(Runner))
            {
                Interactable.Hover hover = currentInteraction.hovers[heldInteractable];
                if (Runner.IsServer) InvokeActions(hover.actions, Object.InputAuthority);
                ResetInteractions();
                if (HasInputAuthority)
                {
                    hover.networkSettings.clientAction.Invoke();
                }
            }
        }
    }

    void InvokeActions(Interactable.Action[] actions, PlayerRef player)
    {
        bool isServer = rm.nRunner.IsServer;
        foreach (Interactable.Action action in actions)
        {
            action.Invoke(player);
        }
    }

    private void OnInteract1(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            rm.interactionPressed = true;
            rm.interactionKey = (int)Interactable.InteractKey.Interact1;
            return;
        }
        rm.interactionPressed = false;
    }

    private void OnInteract2(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            rm.interactionPressed = true;
            rm.interactionKey = (int)Interactable.InteractKey.Interact2;
            return;
        }
        rm.interactionPressed = false;
    }

    private void OnInteract3(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            rm.interactionPressed = true;
            rm.interactionKey = (int)Interactable.InteractKey.Interact3;
            return;
        }
        rm.interactionPressed = false;
    }

    /// <summary>
    /// Sets the current interaction
    /// </summary>
    void CastRay()
    {
        Vector3 trackedPosition = new Vector3(rb.position.x, trackedTransform.position.y, rb.position.z);
        RaycastHit hit;
        if (Physics.Raycast(trackedPosition, forwardDirection, out hit, range, (int)interactableMask)) // Raycast interactable
        {
            RaycastHit eHit;
            if (Physics.Raycast(trackedPosition, forwardDirection, out eHit, range, (int)environmentMask))
            {
                if (eHit.distance < hit.distance) // Raycast environment, if the environment blocks the interactable, reset interactions and stop
                {
                    ResetInteractions();
                    return;
                }
            }
            if (hit.collider.gameObject != currentInteractable) // Executes if we are looking at an interactable
            {
                if (!hit.collider.gameObject.GetComponent<Interactable>().canInteract) return; // Can't interact, return
                if (currentInteraction != null && HasInputAuthority) currentInteraction.UnglowMaterials(); // Stop making the materials glow
                currentInteractable = hit.collider.gameObject;
                currentInteraction = currentInteractable.GetComponent<Interactable>();
                DisplayInteraction(currentInteraction);
                if (HasInputAuthority)
                {
                    CrosshairManager.instance.AddCrosshair(0, 0);
                    currentInteraction.GlowMaterials();
                }
                return;
            }
            return;
        }
        if (currentInteractable != null) ResetInteractions();
    }

    public void ResetInteractions()
    {
        if (currentInteraction != null && HasInputAuthority) currentInteraction.UnglowMaterials();
        currentInteractable = null;
        currentInteraction = null;
        timerRunning = false;
        serverTimer = TickTimer.None;

        if (!HasInputAuthority) return; // Client interaction reset
        StopAllCoroutines();
        timer = 0f; // Client timer
        iui.StopHighlight(); // UI
        iui.ClearInteractions();
        trackedIndexes.Clear();
        CrosshairManager.instance.RemoveCrosshair(0);
    }

    void InteractionKey(Interactable.InteractKey key)
    {
        if (menuData) return;
        if (timer > 0f) return;
        if (currentInteraction == null) return;
        // Checks each interaction key in the current interaction and sees if the key equals the current key
        int i = 0;
        foreach (Interactable.Hover h in currentInteraction.hovers)
        {
            if (h.interactKey == Interactable.InteractKey.None) continue;
            if (h.interactKey == key)
            {
                if (!h.networkSettings.networked)
                {
                    if (!HasInputAuthority) return;
                    ClientInteractionKey(h, i);
                    return;
                }
                if (h.networkSettings.networked)
                {
                    if (Runner.IsServer) // If server
                    {
                        if (h.delay == 0f) // If the delay is 0, immediately execute the action and return
                        {
                            InvokeActions(h.actions, Object.InputAuthority);
                            ResetInteractions();
                        }
                        else if (!timerRunning)
                        {
                            heldInteractable = i;
                            timerRunning = true;
                            serverTimer = TickTimer.CreateFromSeconds(Runner, h.delay);
                        }
                    }
                    if (HasInputAuthority) // If client
                    {
                        ServerInteractionKey(h, iui.transform.GetChild(i)); // Calls server interaction key from client
                    }
                    return;
                }
            }
            i++;
        }
    }

    void ClientInteractionKey(Interactable.Hover hover, int i)
    {
        if (hover.delay == 0f)
        {
            InvokeActions(hover.actions, Object.InputAuthority);
            return;
        }
        iui.StartHighlight(iui.transform.GetChild(i), hover.delay);
        StartCoroutine(StartTimer(hover.delay, hover));
        return;
    }

    // Executes for the client, server interaction key display stuff
    void ServerInteractionKey(Interactable.Hover hover, Transform ui)
    {
        if (hover.delay == 0f)
        {
            hover.networkSettings.clientAction.Invoke();
            return;
        }
        StartCoroutine(StartServerTimer(ui, hover));
        return;
    }

    // Called when the currentPressed variable changes
    void OnPressedChange()
    {
        if (currentPressed)
        {
            InteractionKey(currentKey);
        }
        else
        {
            EndServerInteraction();
        }
    }

    void EndServerInteraction()
    {
        if (timerRunning)
        {
            serverTimer = TickTimer.None;
            timerRunning = false;
        }
    }

    IEnumerator StartServerTimer(Transform interaction, Interactable.Hover h) // For the client
    {
        float localTimer = 0f;
        while (currentPressed)
        {
            yield return null;
            if (!timerRunning)
            {
                localTimer += Time.deltaTime;
                iui.SetHighlight(interaction, localTimer / h.delay);
            }
            else
            {
                if (serverTimer.Expired(Runner))
                {
                    iui.SetHighlight(interaction, 1f);
                    continue;
                }
                float percent = (h.delay - (float)serverTimer.RemainingTime(Runner)) / h.delay;
                iui.SetHighlight(interaction, percent);
            }
        }
        iui.StopHighlight();
    }

    IEnumerator StartTimer(float length, Interactable.Hover h) // For the client
    {
        while (currentPressed)
        {
            yield return null;
            timer += Time.deltaTime;
            if (timer > length)
            {
                InvokeActions(h.actions, Object.InputAuthority);
                if (!iValid)
                {
                    iValid = true;
                    break;
                }
                timer = 0f;
                break;
            }
        }
        timer = 0f;
        iui.StopHighlight();
    }
    
    /// <summary>
    /// Displays the interaction text on the UI
    /// </summary>
    /// <param name="inter"></param>
    void DisplayInteraction(Interactable inter)
    {
        if (!HasInputAuthority) return;
        // Sets to lore of interaction
        Interactable.Hover[] hovers = inter.hovers;
        iui.ClearInteractions();
        trackedIndexes.Clear();
        CrosshairManager.instance.RemoveCrosshair(0);
        int i = 0;
        foreach (Interactable.Hover h in hovers)
        {
            if (h.interactKey != Interactable.InteractKey.None)
            {
                InputAction interactAction = ToInteractAction(h.interactKey).action;
                int bindingIndex = interactAction.GetBindingIndexForControl(interactAction.controls[0]);
                string interactText = InputControlPath.ToHumanReadableString(
                    interactAction.bindings[bindingIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                iui.AddInteraction($"[{interactText}] {h.lore}\n", h.color);
                if (h.trackColor || h.trackLore) trackedIndexes.Add(i);
                i++;
                continue;
            }
            iui.AddInteraction($"{h.lore}\n", h.color);
            if (h.trackColor || h.trackLore) trackedIndexes.Add(i);
            i++;
        }
    }

    /// <summary>
    /// Updates the lore for the interactable
    /// </summary>
    void UpdateTracking()
    {
        if (!HasInputAuthority) return;
        if (currentInteraction == null || trackedIndexes.Count == 0) return;
        Interactable.Hover[] hovers = currentInteraction.hovers;
        foreach (int i in trackedIndexes)
        {
            if (hovers[i].trackColor) iui.SetInteractionColor(i, hovers[i].color);
            if (hovers[i].trackLore)
            {
                if (hovers[i].interactKey == Interactable.InteractKey.None)
                {
                    iui.SetInteractionLore(i, hovers[i].lore);
                } else
                {
                    InputAction interactAction = ToInteractAction(hovers[i].interactKey).action;
                    int bindingIndex = interactAction.GetBindingIndexForControl(interactAction.controls[0]);
                    string interactText = InputControlPath.ToHumanReadableString(
                        interactAction.bindings[bindingIndex].effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice);
                    iui.SetInteractionLore(i, $"[{interactText}] {hovers[i].lore}\n");
                }
            }
        }
    }

    InputActionReference ToInteractAction(Interactable.InteractKey key)
    {
        if (key == Interactable.InteractKey.None) return null;
        return interactActions[(int)key-1];
    }
}
