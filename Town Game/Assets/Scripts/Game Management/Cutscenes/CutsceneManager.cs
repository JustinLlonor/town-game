using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneManager : NetworkBehaviour
{
    [Networked, Capacity(15)] public NetworkLinkedList<SeqScene> timeline => default;
    [Networked] public float cutsceneProgress { get; set; } = -1f;
    private float clientProgress = -1f;
    public int editingIndex;
    [ContextMenuItem("Add Camera Shot", "AddCameraShot")]
    [ContextMenuItem("Add Text", "AddText")]
    [ContextMenuItem("Add Black Screen", "AddBlackScreen")]
    public SeqSceneRef[] sequenceSceneRefs;

    private void Update()
    {
        if ((cutsceneProgress == -1f) && !(clientProgress == -1f))
        {
            clientProgress = -1f;
        }
        if (cutsceneProgress == -1f) return;
        if (cutsceneProgress > clientProgress) clientProgress = cutsceneProgress;
        clientProgress += Time.deltaTime;
    }

    private void AddText()
    {
        sequenceSceneRefs[editingIndex].sceneElements.Add(new TextEffect());
    }

    private void AddCameraShot()
    {
        sequenceSceneRefs[editingIndex].sceneElements.Add(new CameraShot());
    }

    private void AddBlackScreen()
    {
        sequenceSceneRefs[editingIndex].sceneElements.Add(new BlackScreenEffect());
    }
}
