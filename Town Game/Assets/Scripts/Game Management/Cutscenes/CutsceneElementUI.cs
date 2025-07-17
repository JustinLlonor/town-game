using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneElementUI : MonoBehaviour
{
    public RectTransform[] anchoredTransforms;
    private Dictionary<SceneElement, GameObject> elements = new Dictionary<SceneElement, GameObject>();

    public GameObject InstantiateElement(SceneElement element, GameObject prefab, CutsceneAnchor anchor, Vector2 anchoredPosition)
    {
        GameObject elementObject = Instantiate(prefab, anchoredTransforms[(int)anchor]);
        ((RectTransform)elementObject.transform).anchoredPosition = anchoredPosition;
        elements.Add(element, elementObject);
        return elementObject;
    }

    public void DestroyElement(SceneElement element)
    {
        if (elements.ContainsKey(element))
        {
            Destroy(elements[element]);
            elements.Remove(element);
        }
    }

    public void ClearCutsceneUI()
    {
        foreach (GameObject gameObject in elements.Values)
        {
            Destroy(gameObject);
        }
        elements.Clear();
    }
}
