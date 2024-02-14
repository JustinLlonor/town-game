using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractableFinder : MonoBehaviour
{
    [Header("Masks")]
    public LayerMask interactableMask;
    public LayerMask environmentMask;
    [Header("Settings")]
    public float range = 2f;
    [Header("References")]
    public TextMeshProUGUI interactText;
    public InteractableUI iui;

    [HideInInspector] public bool iValid = true;
    GameObject currentInteractable = null;
    CursorManager cm;
    Interactable currentInteraction;
    float timer = 0f;

    private void Awake()
    {
        cm = FindObjectOfType<CursorManager>();
    }

    private void Update()
    {
        CastRay();
        InteractionKey();
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

    void ResetInteractions()
    {
        if (currentInteraction != null) currentInteraction.UnglowMaterials();
        currentInteractable = null;
        currentInteraction = null;
        StopAllCoroutines();
        timer = 0f;
        iui.StopHighlight();
        iui.ClearInteractions();
        CrosshairManager.instance.RemoveCrosshair(0);
    }

    void InteractionKey()
    {
        if (!cm.isLocked) return;
        if (timer > 0f) return;
        if (currentInteraction != null)
        {
            int i = 0;
            foreach (Interactable.Hover h in currentInteraction.hovers)
            {
                if (Input.GetKeyDown(h.key))
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
        while (Input.GetKey(h.key))
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
        foreach (Interactable.Hover h in hovers)
        {
            iui.AddInteraction($"[{h.key}] {h.lore}\n");
        }
    }
}
