using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneElementUI : MonoBehaviour
{
    public RectTransform[] anchoredTransforms;
    private Dictionary<SceneElementInfo, GameObject> elements = new Dictionary<SceneElementInfo, GameObject>();

    public GameObject InstantiateElement(SceneElementInfo eInfo, GameObject prefab, CutsceneAnchor anchor, Vector2 anchoredPosition)
    {
        SceneElement element = eInfo.element;
        GameObject elementObject = Instantiate(prefab, anchoredTransforms[(int)anchor]);
        ((RectTransform)elementObject.transform).anchoredPosition = anchoredPosition;
        elements.Add(eInfo, elementObject);
        return elementObject;
    }

    public void DestroyElement(SceneElementInfo eInfo)
    {
        if (elements.ContainsKey(eInfo))
        {
            Destroy(elements[eInfo]);
            elements.Remove(eInfo);
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
