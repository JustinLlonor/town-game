using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeManager : MonoBehaviour
{
    public static AttributeManager i;

    public AttributeInfo[] attributeInfos;
    private Dictionary<ItemAttribute, AttributeInfo> attributeDictionary = new Dictionary<ItemAttribute, AttributeInfo>();

    [System.Serializable]
    public struct AttributeInfo
    {
        public ItemAttribute attribute;
        public Texture2D texture;
    }

    private void Awake()
    {
        i = this;
        foreach (AttributeInfo info in attributeInfos)
        {
            attributeDictionary.Add(info.attribute, info);
        }
    }

    public Texture2D GetAttributeTexture(ItemAttribute attribute)
    {
        if (!attributeDictionary.ContainsKey(attribute)) return null;
        return attributeDictionary[attribute].texture;
    }
}