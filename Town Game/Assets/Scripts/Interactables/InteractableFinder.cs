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
    bool init = false;

    // Interactable revamp variables
    [Networked] public bool canInteract { get; set; } = true; // If the player has the ability to interact
    [Networked] public bool lookingAtInteract { get; set; } = false; // If the player is looking at an action holder
    [Networked] public NetworkBehaviourId viewedActionHolder { get; set; } // The current viewed action holder
    [Networked] public int interactionIndex { get; set; } = -1; // -1 if interaction key isn't pressed, 0+ if it is pressed on something
    [Networked] public float interactTime { get; set; } // The amount of time held for the interaction
    [Networked] public bool pressFinished { get; set; } = false; // if the current action has finished being pressed 

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
            if (currentPressed != previousPressed)
            {
                previousPressed = currentPressed;
                OnPressedChange();
            }
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
            if (HasInputAuthority) iui.DisplayActionHolder(foundHolder, this);
            if (foundHolder == null)
            {
                ResetInteractions();
                return;
            }
            NetworkBehaviourId foundHolderId = foundHolder.Id;
            if (foundHolderId != viewedActionHolder)
            {
                viewedActionHolder = foundHolderId;
            }
        }
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
        lookingAtInteract = false;
        // UI stuff
        if (!HasInputAuthority) return; // Client interaction reset
        iui.DisplayActionHolder(null, this);
        //StopAllCoroutines();
    }

    void InteractionKey(Interactable.InteractKey key)
    {
        
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
            EndInteraction();
        }
    }

    void EndInteraction()
    {
        
    }

    InputActionReference ToInteractAction(Interactable.InteractKey key)
    {
        if (key == Interactable.InteractKey.None) return null;
        return interactActions[(int)key-1];
    }
}
