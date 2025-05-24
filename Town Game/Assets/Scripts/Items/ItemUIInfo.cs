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
    public GameObject panelObject;
    public TextMeshProUGUI nameText;
    public RawImage icon;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI roomText;
    public TextMeshProUGUI descriptionText;
    public Animator panelAnimator;
    public Billboard billboard;
    public float yOffset = 0.35f;

    Item item;
    bool init = false;
    public bool descriptionRevealed = false;
    bool dataInit = false;

    private void OnEnable()
    {
        if (init) return;
        init = true;
        panelObject.SetActive(false);
        interactable.onLook += Look;
        interactable.onLookAway += Unlook;
    }

    private void Look()
    {
        SetDistance();
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

    private void Unlook()
    {
        panelAnimator.SetBool("IsLooking", false);
        panelAnimator.SetBool("DescriptionShown", false);
    }

    public void HideObject()
    {
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
    }

    void SetDistance()
    {
        Vector3 itemPosition = itemPhys.transform.position;
        float distance = 0.86f + yOffset;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        transform.position = new Vector3(itemPosition.x, itemPosition.y + distance, itemPosition.z);
    }
}
