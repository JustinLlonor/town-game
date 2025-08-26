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
    public InputActionReference nextPageAction;
    bool previousCanInteract = true;

    [HideInInspector] public Player player;
    [HideInInspector] public Transform trackedTransform;
    [HideInInspector] public Rigidbody rb; // To find rb location because only rb location is accurately networked
    RunnerManager rm;

    [HideInInspector] public Vector3 forwardDirection;
    [HideInInspector] public bool menuData = false;
    bool init = false;

    // Interactable revamp variables
    [Networked] public bool canInteract { get; set; } = true; // If the player has the ability to interact
    [Networked] public bool lookingAtInteract { get; set; } = false; // If the player is looking at an action holder
    [Networked] public NetworkBehaviourId viewedActionHolder { get; set; } // The current viewed action holder
    [Networked] public int interactionIndex { get; set; } = -1; // -1 if interaction key isn't pressed, 0+ if it is pressed on something
    private int previousInteractionIndex = -1;
    [Networked] public float interactTime { get; set; } // The amount of time held for the interaction
    [Networked] public bool pressFinished { get; set; } = false; // if the current action has finished being pressed 
    [Networked] public bool holdAction { get; set; } = false;
    [Networked, Capacity(10)] public NetworkLinkedList<int> serverInteractions => default; // Indexes of the server interactions in action holder
    public List<int> clientInteractions = new List<int>(); // Indexes of the client interactions in action holder
    // The interactions from the server and client that are displayed on this client. 
    public List<int> displayInteractions = new List<int>();
    public int displayPage = 0;

    public override void Spawned()
    {
        if (IsProxy) Destroy(this);
        if (HasInputAuthority) UIManager.instance.OnUIOpen += ResetInteractionUIOpen;
        init = true;
        iui = FindFirstObjectByType<InteractableUI>();
        rm = FindFirstObjectByType<RunnerManager>();
        if (!HasInputAuthority) return;
        // Input manager interaction functions, sent to runner manager and sent back to player
        InputManager inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onInteract1 += OnInteract1;
        inputManager.onInteract2 += OnInteract2;
        inputManager.onInteract3 += OnInteract3;
        inputManager.onNextPage += OnNextPage;
    }

    public void SetCanInteract(bool interactability)
    {
        canInteract = interactability;
    }

    public override void Render()
    {
        if (!init) return;
        // canInteract change detector
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
                CastRay();
            }
            // Interaction index change detector
            if (previousInteractionIndex != interactionIndex)
            {
                previousInteractionIndex = interactionIndex;
                Interact(interactionIndex);
            }
            InteractHold();
        }
    }

    private void Interact(int iIndex)
    {
        if (!lookingAtInteract) return;
        pressFinished = false;
        interactTime = 0f;
        holdAction = false;
        if (iIndex == -1)
        {
            return;
        }
        ActionHolder holder;
        if (!Runner.TryFindBehaviour(viewedActionHolder, out holder)) return;
        if (iIndex >= holder.actions.Length) return;
        IntAction action = holder.actions[iIndex];
        // Client actions
        if (action.isClient)
        {
            if (HasInputAuthority) action.onInteract?.Invoke(player);
            return;
        }
        NIActionInfo info = holder.actionInfo[action.actionInfoIndex];
        if (!info.CanInteract(player.owner)) return;
        // Length 0 server actions
        float interactLength = info.GetInteractLength(player.owner);
        if (interactLength == 0f)
        {
            action.onInteract?.Invoke(player);
            return;
        }
        // Delayed length server actions
        holdAction = true;
    }

    private void InteractHold()
    {
        if (pressFinished) return;
        if (!holdAction) return;
        if (!lookingAtInteract) return;
        if (interactionIndex == -1) return;
        ActionHolder holder;
        if (!Runner.TryFindBehaviour(viewedActionHolder, out holder)) return;
        IntAction action = holder.actions[interactionIndex];
        NIActionInfo info = holder.actionInfo[action.actionInfoIndex];
        float length = info.GetInteractLength(player.owner);
        interactTime += Runner.DeltaTime;
        if (interactTime > length)
        {
            pressFinished = true;
            if (info.CanInteract(player.owner)) action.onInteract?.Invoke(player);
        }
    }

    private void InteractIndex(int index, float value)
    {
        if (value == 1f && lookingAtInteract)
        {
            rm.interactionPressed = true;
            int currentIndex = displayPage * 3 + index;
            if (currentIndex >= displayInteractions.Count) return;
            rm.interactIndex = displayInteractions[currentIndex];
            return;
        }
        rm.interactionPressed = false;
        rm.interactIndex = -1;
    }

    private void OnInteract1(InputValue iv)
    {
        InteractIndex(0, iv.Get<float>());
    }

    private void OnInteract2(InputValue iv)
    {
        InteractIndex(1, iv.Get<float>());
    }

    private void OnInteract3(InputValue iv)
    {
        InteractIndex(2, iv.Get<float>());
    }

    private void OnNextPage()
    {
        // Increases next page variable, wraps when it gets too big
        if (displayInteractions.Count <= 3) return;
        int maxIndex = Mathf.CeilToInt(displayInteractions.Count / 3f) - 1;
        displayPage++;
        if (displayPage > maxIndex) displayPage = 0;
        rm.interactIndex = -1;
    }

    /// <summary>
    /// Sets the current interaction
    /// </summary>
    void CastRay()
    {
        if (menuData) return; // return if the menu is open
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
            ActionHolder foundHolder = hit.collider.GetComponent<ActionHolder>();
            if (viewedActionHolder != foundHolder.Id)
            {
                if (HasInputAuthority)
                {
                    displayPage = 0;
                    ActionHolder viewedHolder;
                    if (Runner.TryFindBehaviour(viewedActionHolder, out viewedHolder))
                    {
                        viewedHolder.onUnlook?.Invoke();
                    }
                }
                interactionIndex = -1;
                pressFinished = false;
                interactTime = 0f;
            }
            // Set interaction and display interaction lists
            SetInteractions(foundHolder);
            if (foundHolder != null) SetDisplayInteractions();
            if (HasInputAuthority) iui.DisplayActionHolder(foundHolder, this);
            if (foundHolder == null)
            {
                ResetInteractions();
                return;
            }
            NetworkBehaviourId foundHolderId = foundHolder.Id;
            // This is set to true when the player looks at a different action holder, or when
            // the player looks at an action holder after having not looked at an action holder
            bool initiateOnLook = false;
            if (foundHolderId != viewedActionHolder)
            {
                viewedActionHolder = foundHolderId;
                initiateOnLook = true;
            }
            if (!lookingAtInteract) initiateOnLook = true;
            if (initiateOnLook && HasInputAuthority)
            {
                displayPage = 0;
                foundHolder.onLook?.Invoke();
            } 
            lookingAtInteract = true;
        }
        else
        {
            ResetInteractions(); // reset if not looking at interactable thingie
        }
    }

    /// <summary>
    /// Sets the server and client interaction variables for this player
    /// </summary>
    /// <param name="holder"></param>
    private void SetInteractions(ActionHolder holder)
    {
        if (holder == null) return;
        if (holder.despawned) return;
        serverInteractions.Clear();
        clientInteractions.Clear();
        for (int i = 0; i < holder.actions.Length; i++)
        {
            IntAction action = holder.actions[i];
            if (!action.isClient)
            {
                // Gets the action info from the action index
                NIActionInfo info = holder.actionInfo[action.actionInfoIndex];
                if (info.CanInteract(player.owner))
                {
                    serverInteractions.Add(i);
                }
            }
            else
            {
                if (action.enabled)
                {
                    clientInteractions.Add(i);
                }
            }
        }
    }

    private void SetDisplayInteractions()
    {
        if (!HasInputAuthority) return;
        displayInteractions.Clear();
        // Code for sorting the client and server interactions from lowest to highset in the display interactions
        foreach (int interaction in serverInteractions)
        {
            displayInteractions.Add(interaction);
        }
        foreach (int interaction in clientInteractions)
        {
            displayInteractions.Add(interaction);
        }
        displayInteractions.Sort();
    }

    private void ResetInteractionUIOpen(int i)
    {
        ResetInteractions();
    }

    /// <summary>
    /// Resets interaction variables
    /// </summary>
    public void ResetInteractions()
    {
        displayPage = 0;
        pressFinished = false;
        if (lookingAtInteract)
        {
            lookingAtInteract = false;
            if (HasInputAuthority)
            {
                ActionHolder holder;
                if (Runner.TryFindBehaviour(viewedActionHolder, out holder))
                {
                    if (!holder.despawned)
                    {
                        holder.onUnlook?.Invoke();
                    }
                }
            }
        }
        serverInteractions.Clear();
        clientInteractions.Clear();
        displayInteractions.Clear();
        interactTime = 0f;
        // UI stuff
        if (!HasInputAuthority) return; // Client interaction reset
        iui.DisplayActionHolder(null, this);
        //StopAllCoroutines();
    }

    public string ToInteractKey(int keyIndex)
    {
        if (keyIndex == -1) return "";
        InputAction actionRef = interactActions[keyIndex].action;
        int bindingIndex = actionRef.GetBindingIndexForControl(actionRef.controls[0]);
        string interactText = InputControlPath.ToHumanReadableString(
                    actionRef.bindings[bindingIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
        return interactText;
    }

    public string GetScrollKey()
    {
        InputAction actionRef = nextPageAction.action;
        int bindingIndex = actionRef.GetBindingIndexForControl(actionRef.controls[0]);
        string interactText = InputControlPath.ToHumanReadableString(
                    actionRef.bindings[bindingIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
        return interactText;
    }
}
