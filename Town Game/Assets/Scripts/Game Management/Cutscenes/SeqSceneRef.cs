using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SeqSceneRef
{
    [SerializeReference]
    public List<SceneElement> sceneElements = new List<SceneElement>();
}
