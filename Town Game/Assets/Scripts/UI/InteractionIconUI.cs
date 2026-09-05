using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionIconUI : MonoBehaviour
{
    public GameObject iconPrefab;

    public void DisplayFilterInfo(FilterInfo info, int delta = 0)
    {
        foreach (Transform child in transform)
        {
            if (child.GetSiblingIndex() <= 1) continue;
            Destroy(child.gameObject);
        }
        if (info.filteredItem != null)
        {
            InstantiateItem(info.filteredItem, delta);
        }
        if (info.filteredAttributes != null)
        {
            InstantiateAttributes(info.filteredAttributes, delta);
        }
    }

    private void InstantiateItem(Item item, int delta)
    {
        GameObject go = Instantiate(iconPrefab, transform);
        ItemIconUI iiui = go.GetComponent<ItemIconUI>();
        iiui.SetItemIcon(item);
        iiui.SetArrowImages(delta);
    }

    private void InstantiateAttributes(List<ItemAttribute> attributes, int delta)
    {
        foreach (ItemAttribute attribute in attributes)
        {
            GameObject go = Instantiate(iconPrefab);
            ItemIconUI iiui = go.GetComponent<ItemIconUI>();
            iiui.SetAttributeIcon(attribute);
            iiui.SetArrowImages(delta);
        }
    }
}
