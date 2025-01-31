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

    [HideInInspector] public bool iValid = true;
    [HideInInspector] public Interactable currentInteraction;
    [HideInInspector] public Player player;
    [HideInInspector] public Transform trackedTransform;
    [HideInInspector] public Rigidbody rb;
    GameObject currentInteractable = null;
    float timer = 0f;
    [Networked] float serverTimer { get; set; } // Interaction timer on the server
    [Networked] int interactPressed { get; set; } // Interaction key that is pressed
    [Networked] bool currentPressed { get; set; } = false; // If the thing is currently pressed
    List<int> trackedIndexes = new List<int>();
    Vector3 forwardDirection;
    bool menuData = false;

    public override void Spawned()
    {
        if (IsProxy) Destroy(this);
        if (HasInputAuthority) UIManager.instance.OnUIOpen += ResetInteractions;
        iui = FindObjectOfType<InteractableUI>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            menuData = data.menu;
            forwardDirection = Quaternion.Euler(data.camDirectionX, data.camDirection, 0f) * Vector3.forward; // orientation/camDirection is mouse x
            if (!Runner.IsResimulation)
            {
                if (!menuData) CastRay(); // If the menu isn't open for the player, cast a ray
                UpdateTracking();
            }
        }
    }

    private void OnInteract1(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            currentPressed = true;
            InteractionKey(Interactable.InteractKey.Interact1);
            return;
        }
        currentPressed = false;
    }

    private void OnInteract2(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            currentPressed = true;
            InteractionKey(Interactable.InteractKey.Interact2);
            return;
        }
        currentPressed = false;
    }

    private void OnInteract3(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            currentPressed = true;
            InteractionKey(Interactable.InteractKey.Interact3);
            return;
        }
        currentPressed = false;
    }

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
        serverTimer = 0f;
        currentInteractable = null;
        currentInteraction = null;

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
        if (UIManager.instance.uiOpened) return;
        if (timer > 0f) return;
        if (currentInteraction != null)
        {
            int i = 0;
            foreach (Interactable.Hover h in currentInteraction.hovers)
            {
                if (h.interactKey == Interactable.InteractKey.None) continue;
                if (h.interactKey == key)
                {
                    if (h.delay == 0f)
                    {
                        h.action.Invoke();
                        ResetInteractions();
                        return;
                    }
                    iui.StartHighlight(iui.transform.GetChild(i), h.delay);
                    StartCoroutine(StartTimer(h.delay, h));
                }
                i++;
            }
        }
    }

    IEnumerator StartTimer(float length, Interactable.Hover h)
    {
        while (currentPressed)
        {
            yield return null;
            timer += Time.deltaTime;
            if (timer > length)
            {
                h.action.Invoke();
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
