using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneManager : NetworkBehaviour
{
    [Networked, Capacity(15)] public NetworkLinkedList<SeqScene> timeline => default;
    [Networked] public float cutsceneProgress { get; set; } = -1f;
    private float clientProgress = -1f;
    public int editingIndex;
    //[ContextMenuItem("Add Camera Shot", "AddCameraShot")]
    [ContextMenuItem("Add Text", "AddText")]
    [ContextMenuItem("Add Black Screen", "AddBlackScreen")]
    [ContextMenuItem("Add Zoom Effect", "AddZoomEffect")]
    public SeqSceneRef[] sequenceSceneRefs;
    // Editor functions
    #region
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

    private void AddZoomEffect()
    {
        sequenceSceneRefs[editingIndex].sceneElements.Add(new ZoomEffect());
    }
    #endregion
    /// <summary>
    /// Called whenever a new sequence starts. 
    /// The returned list of SeqScenes is appended to the sequence.
    /// </summary>
    public SequenceEvent onSequenceStart;
    public CutsceneReader cutsceneReader;
    public float timelineLength { get; private set; }

    public delegate List<SeqScene> SequenceEvent(string sequenceName);

    private void Update()
    {
        // Stop reading if we just exited
        if ((cutsceneProgress == -1f) && !(clientProgress == -1f))
        {
            clientProgress = -1f;
            cutsceneReader.StopReading();
        }
        if (cutsceneProgress == -1f) return;
        if ((cutsceneProgress == -1f) && (clientProgress >= 0f))
        {
            cutsceneReader.StartReading();
            timelineLength = GetSequenceLength();
        }
        // Sync client progress with server
        if (cutsceneProgress > clientProgress) clientProgress = cutsceneProgress;
        clientProgress += Time.deltaTime;
        cutsceneReader.ReadCutscene(clientProgress);
    }

    private float GetSequenceLength()
    {
        float length = 0f;
        foreach (SeqScene scene in timeline)
        {
            length += sequenceSceneRefs[scene.sceneIndex].GetLength();
        }
        return length;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer) return;
        if (cutsceneProgress == -1f) return;
        cutsceneProgress += Runner.DeltaTime;
    }

    /// <summary>
    /// Starts a sequence for a particular part of day. If the sequence has scenes, then this returns true. False otherwise
    /// </summary>
    /// <param name="sequenceName"></param>
    /// <returns></returns>
    public bool StartSequence(string sequenceName)
    {
        List<SeqScene> scenes = GetSeqSceneList(sequenceName);
        if (scenes.Count == 0) return false;
        timeline.Clear();
        foreach (SeqScene scene in scenes)
        {
            timeline.Add(scene);
        }
        cutsceneProgress = 0f;
        return true;
    }

    private List<SeqScene> GetSeqSceneList(string sequenceName)
    {
        Delegate[] startDelegates =  onSequenceStart.GetInvocationList();
        List<SeqScene> output = new List<SeqScene>();
        for (int i = 0; i < startDelegates.Length; i++)
        {
            List<SeqScene> returnedScenes = (List<SeqScene>)startDelegates[i].DynamicInvoke(sequenceName);
            foreach (SeqScene scene in returnedScenes)
            {
                output.Add(scene);
            }
        }
        return output;
    }

    public SeqSceneRef GetRef(SeqScene scene)
    {
        return sequenceSceneRefs[scene.sceneIndex];
    }
}
