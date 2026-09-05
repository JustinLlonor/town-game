using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SeqSceneRef
{
    public string name;
    [SerializeReference]
    public List<SceneElement> sceneElements = new List<SceneElement>();
    private float calculatedLength = -1f;

    public float GetLength()
    {
        if (calculatedLength != -1f) return calculatedLength;
        float farthestEnd = 0f;
        foreach (SceneElement element in sceneElements)
        {
            float currentEnd = element.time + element.length;
            if (currentEnd > farthestEnd)
            {
                farthestEnd = currentEnd;
            }
        }
        calculatedLength = farthestEnd;
        return farthestEnd;
    }
}
