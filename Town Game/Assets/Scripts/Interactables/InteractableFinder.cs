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

    GameObject currentInteractable = null;
    CursorManager cm;
    Interactable currentInteraction;

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
        ResetInteractions();
    }

    void ResetInteractions()
    {
        if (currentInteraction != null) currentInteraction.UnglowMaterials();
        currentInteractable = null;
        currentInteraction = null;
        interactText.gameObject.SetActive(false);
        CrosshairManager.instance.RemoveCrosshair(0);
    }

    void InteractionKey()
    {
        if (!cm.isLocked) return;
        if (currentInteraction != null)
        {
            foreach (Interactable.Hover h in currentInteraction.hovers)
            {
                if (Input.GetKeyDown(h.key))
                {
                    h.action.Invoke();
                }
            }
        }
    }

    void DisplayInteraction(Interactable inter)
    {
        // Sets to lore of interaction
        Interactable.Hover[] hovers = inter.hovers;

        string iTxt = "";
        foreach (Interactable.Hover h in hovers)
        {
            iTxt = $"{iTxt}[{h.key}] {h.lore}\n";
        }

        interactText.gameObject.SetActive(true);
        interactText.text = iTxt;
    }
}
