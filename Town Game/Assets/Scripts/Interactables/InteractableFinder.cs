using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableFinder : MonoBehaviour
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
    GameObject currentInteractable = null;
    float timer = 0f;
    bool currentPressed = false;
    List<int> trackedIndexes = new List<int>();

    private void Start()
    {
        UIManager.instance.OnUIOpen += ResetInteractions;
    }

    private void Update()
    {
        if (!UIManager.instance.uiOpened) CastRay();
    }

    private void FixedUpdate()
    {
        UpdateTracking();
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
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, range, (int)interactableMask))
        {
            RaycastHit eHit;
            if (Physics.Raycast(transform.position, transform.forward, out eHit, range, (int)environmentMask))
            {
                if (eHit.distance < hit.distance)
                {
                    ResetInteractions();
                    return;
                }
            }
            if (hit.collider.gameObject != currentInteractable)
            {
                if (!hit.collider.gameObject.GetComponent<Interactable>().canInteract) return;
                if (currentInteraction != null) currentInteraction.UnglowMaterials();
                currentInteractable = hit.collider.gameObject;
                currentInteraction = currentInteractable.GetComponent<Interactable>();
                DisplayInteraction(currentInteraction);
                CrosshairManager.instance.AddCrosshair(0, 0);
                currentInteraction.GlowMaterials();
                return;
            }
            return;
        }
        if (currentInteractable != null) ResetInteractions();
    }

    public void ResetInteractions()
    {
        if (currentInteraction != null) currentInteraction.UnglowMaterials();
        currentInteractable = null;
        currentInteraction = null;
        StopAllCoroutines();
        timer = 0f;
        iui.StopHighlight();
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
                CrosshairManager.instance.RemoveCrosshair(0);
                iui.ClearInteractions();
                timer = 0f;
                break;
            }
        }
        timer = 0f;
        iui.StopHighlight();
    }

    void DisplayInteraction(Interactable inter)
    {
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

    void UpdateTracking()
    {
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
