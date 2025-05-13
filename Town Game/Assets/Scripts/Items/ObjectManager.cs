using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows for reusable assets to be sent across the network
/// </summary>
public class ObjectManager : MonoBehaviour
{
    public static ObjectManager i;
    public Item[] items;
    public Dictionary<string, Item> itemSearch = new Dictionary<string, Item>();
    public Texture2D[] textures;
    public Dictionary<string, Texture2D> texSearch = new Dictionary<string, Texture2D>();
    public Clothing[] clothings;
    public Dictionary<string, Clothing> clothingSearch = new Dictionary<string, Clothing>();
    public GameObject[] prefabs;
    public Dictionary<string, GameObject> prefabSearch = new Dictionary<string, GameObject>();

    private void Awake()
    {
        i = this;
        foreach (Item item in items) itemSearch.Add(item.name, item);
        foreach (Texture2D tex in textures) texSearch.Add(tex.name, tex);
        foreach (Clothing clothing in clothings) clothingSearch.Add(clothing.name, clothing);
        foreach (GameObject prefab in prefabs) prefabSearch.Add(prefab.name, prefab);
    }
}
