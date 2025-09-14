using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class ItemUIInfo : MonoBehaviour
{
    [HideInInspector] public ItemPhys itemPhys;
    [HideInInspector] public Interactable interactable;
    public ActionHolder actionHolder;
    public GameObject panelObject;
    public TextMeshProUGUI nameText;
    public RawImage icon;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI roomText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI attributeText;
    public Animator panelAnimator;
    public Billboard billboard;
    public Canvas canvas;
    public WorldUIFollow follow;

    Item item;
    bool init = false;
    public bool descriptionRevealed = false;
    bool dataInit = false;

    private void OnEnable()
    {
        if (init) return;
        init = true;
        panelObject.SetActive(false);
        actionHolder.onLook += Look;
        actionHolder.onLookContinue += LookContinue;
        actionHolder.onUnlook += Unlook;
        /**
        interactable.onLook += Look;
        interactable.onLookAway += Unlook;
        */
    }

    private void Look(Vector3 position)
    {
        canvas.enabled = true;
        follow.enabled = true;
        follow.SetPosition(position);
        if (!dataInit)
        {
            dataInit = true;
            item = ObjectManager.i.itemSearch[itemPhys.itemName.ToString()];
            UpdateData();
        }
        panelObject.SetActive(true);
        billboard.enabled = true;
        descriptionRevealed = false;
        panelAnimator.Play("Show"); // Overrides hiding animation
        panelAnimator.SetBool("IsLooking", true);
        panelAnimator.SetBool("DescriptionShown", false);
    }

    private void LookContinue(Vector3 position)
    {
        follow.SetTarget(position);
    }

    private void Unlook()
    {
        follow.enabled = false;
        panelAnimator.SetBool("IsLooking", false);
        panelAnimator.SetBool("DescriptionShown", false);
    }

    public void HideObject()
    {
        canvas.enabled = false;
        panelObject.SetActive(false);
        billboard.enabled = false;
    }

    public void DescriptionReveal()
    {
        descriptionRevealed = !descriptionRevealed;
        panelAnimator.SetBool("DescriptionShown", descriptionRevealed);
    }

    void UpdateData()
    {
        nameText.text = item.name;
        typeText.text = item.GetItemType();
        icon.texture = item.icon;
        string ownership = itemPhys.GetOwnership();
        if (ownership.IsNullOrEmpty())
        {
            ownership = "Belongs to nowhere";
        }
        else
        {
            ownership = "Belongs to " + ownership;
        }
        roomText.text = ownership;
        descriptionText.text = item.description;
        string attributes = "";
        bool firstString = true;
        foreach (ItemAttribute attribute in item.attributes)
        {
            if (!firstString) attributes += ", ";
            attributes += attribute.ToReadable();
            if (firstString) firstString = false;
        }
        attributeText.text = attributes;
    }
}
