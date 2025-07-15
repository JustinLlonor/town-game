using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SeqSceneRef
{
    public string name;
    [SerializeReference]
    public List<SceneElement> sceneElements = new List<SceneElement>();
}
