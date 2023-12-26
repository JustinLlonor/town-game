using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public Item[] items;
    public Dictionary<string, Item> itemSearch = new Dictionary<string, Item>();

    private void Awake()
    {
        foreach (Item item in items)
        {
            itemSearch.Add(item.name, item);
        }
    }
}
