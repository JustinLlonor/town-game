using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public Item[] items;
    public Dictionary<string, Item> itemSearch = new Dictionary<string, Item>();
    public Texture2D[] textures;
    public Dictionary<string, Texture2D> texSearch = new Dictionary<string, Texture2D>();
    public Clothing[] clothings;
    public Dictionary<string, Clothing> clothingSearch = new Dictionary<string, Clothing>();

    private void Awake()
    {
        foreach (Item item in items) itemSearch.Add(item.name, item);
        foreach (Texture2D tex in textures) texSearch.Add(tex.name, tex);
        foreach (Clothing clothing in clothings) clothingSearch.Add(clothing.bodyPart.ToString(), clothing);
    }
}
