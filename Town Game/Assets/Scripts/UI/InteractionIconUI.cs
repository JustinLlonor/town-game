using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionIconUI : MonoBehaviour
{
    public GameObject iconPrefab;

    public void DisplayFilterInfo(FilterInfo info)
    {
        foreach (Transform child in transform)
        {
            if (child.GetSiblingIndex() <= 1) continue;
            Destroy(child.gameObject);
        }
        if (info.filteredItem != null)
        {
            InstantiateItem(info.filteredItem);
        }
        if (info.filteredAttributes != null)
        {
            InstantiateAttributes(info.filteredAttributes);
        }
    }

    private void InstantiateItem(Item item)
    {
        GameObject go = Instantiate(iconPrefab, transform);
        ItemIconUI iiui = go.GetComponent<ItemIconUI>();
        iiui.SetItemIcon(item);
    }

    private void InstantiateAttributes(List<ItemAttribute> attributes)
    {
        foreach (ItemAttribute attribute in attributes)
        {
            GameObject go = Instantiate(iconPrefab);
            ItemIconUI iiui = go.GetComponent<ItemIconUI>();
            iiui.SetAttributeIcon(attribute);
        }
    }
}
