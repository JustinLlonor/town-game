using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EntryPointUI : MonoBehaviour
{
    public float fullBarWidth = 21.11f;
    public RectTransform barFill;
    public TextMeshProUGUI title;
    public Transform iconHolder;
    public GameObject itemIconUI;

    /// <summary>
    /// Sets the ui to show the handler info, init = true means it sets the attributes as well
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="init"></param>
    public void SetHandlerInfo(ProgressHandler handler, bool init = false)
    {
        barFill.sizeDelta = new Vector2(((100f - handler.progress) / 100f) * fullBarWidth, barFill.sizeDelta.y);
        if (!init) return; // if not creating, return
        title.text = handler.progressableName;
        // Adds icons for items
        foreach (ItemRate rate in handler.progressProfile.itemRates)
        {
            GameObject iconObject = Instantiate(itemIconUI, iconHolder);
            ItemIconUI iiui = iconObject.GetComponent<ItemIconUI>();
            iiui.SetItemIcon(rate.item);
            iiui.SetArrowImages(handler.progressProfile.GetDelta(-rate.modifiedRate));
        }
        // Adds icons for attributes
        foreach (ItemAttributeRate rate in handler.progressProfile.attributeRates)
        {
            GameObject iconObject = Instantiate(itemIconUI, iconHolder);
            ItemIconUI iiui = iconObject.GetComponent<ItemIconUI>();
            iiui.SetAttributeIcon(rate.attribute);
            iiui.SetArrowImages(handler.progressProfile.GetDelta(-rate.modifiedRate));
        }
    }
}
