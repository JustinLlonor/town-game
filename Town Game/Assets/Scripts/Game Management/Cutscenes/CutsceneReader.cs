using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneReader : NetworkBehaviour
{
    public CutsceneManager cutsceneManager;
    public Transform cutsceneCamTransform;
    public GameObject cutsceneTextPrefab;
    CameraManager cameraManager;
    BlackScreen blackScreen;
    CutsceneElementUI ceUI;
    SeqRefManager srManager;
    SeqScene currentScene = SeqScene.None;
    SeqScene previousScene = SeqScene.None;
    List<SceneElementInfo> previousSceneElements = new List<SceneElementInfo>();
    private float currentLocalTime = -1f;
    private int currentSceneIndex = -1;

    public override void Spawned()
    {
        cameraManager = FindAnyObjectByType<CameraManager>();
        blackScreen = FindAnyObjectByType<BlackScreen>();
        ceUI = FindAnyObjectByType<CutsceneElementUI>();
        srManager = FindAnyObjectByType<SeqRefManager>();
    }

    public void StartReading()
    {
        currentLocalTime = -1f;
        currentScene = SeqScene.None;
        previousScene = SeqScene.None;
        previousSceneElements.Clear();
        cameraManager.SetTrackedCinematicTransform(cutsceneCamTransform);
        cameraManager.ChangeCameraMode(CameraManager.CameraMode.Cinematic);
    }

    public void StopReading()
    {
        ceUI.ClearCutsceneUI();
        cameraManager.ChangeCameraMode(CameraManager.CameraMode.FirstPerson);
    }

    public void ReadCutscene(float progress)
    {
        // List change detector
        List<SceneElementInfo> currentSceneElements = GetElements(progress);
        foreach (SceneElementInfo eInfo in currentSceneElements)
        {
            if (!previousSceneElements.Contains(eInfo))
            {
                AddElement(eInfo);
            }
        }
        foreach (SceneElementInfo eInfo in previousSceneElements)
        {
            if (!currentSceneElements.Contains(eInfo))
            {
                RemoveElement(eInfo);
            }
        }
        previousSceneElements = currentSceneElements;
        foreach (SceneElementInfo eInfo in previousSceneElements)
        {
            ProcessElement(eInfo);
        }
        // Check if the scene changed
        if (!previousScene.Equals(currentScene))
        {
            previousScene = currentScene;
            OnSwitchScene();
        }
    }

    private void OnSwitchScene()
    {
        if (currentScene.Equals(SeqScene.None)) return;
        cutsceneCamTransform.position = currentScene.camPosition;
        cutsceneCamTransform.rotation = currentScene.camRotation;
    }

    /// <summary>
    /// Gets the elements and sets currentScene to whatever scene we are on
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    private List<SceneElementInfo> GetElements(float progress)
    {
        List<SceneElementInfo> output = new List<SceneElementInfo>();
        if (cutsceneManager.timeline.Count == 0) return output;
        // Find the current scene we are working with
        float previousEnding = 0f;
        SeqSceneRef currentSceneRef = null;
        for (int i = 0; i < cutsceneManager.timeline.Count; i++)
        {
            SeqScene scene = cutsceneManager.timeline[i];
            SeqSceneRef sceneRef = cutsceneManager.GetRef(scene);
            if (progress <= previousEnding + sceneRef.GetLength())
            {
                currentScene = cutsceneManager.timeline[i];
                currentSceneRef = sceneRef;
                currentSceneIndex = i;
                break;
            }
            previousEnding += sceneRef.GetLength();
        }
        // Likely happens at end of timeline
        if (currentSceneRef == null)
        {
            currentScene = SeqScene.None;
            currentLocalTime = -1f;
            return output;
        }
        // Set local time to determine local scene calculations
        float localTime = progress - previousEnding;
        // Add all scene elements
        foreach (SceneElement element in currentSceneRef.sceneElements)
        {
            if ((localTime >= element.time) && (localTime <= (element.time + element.length)))
            {
                output.Add(new SceneElementInfo(element, currentSceneIndex));
            }
        }
        currentLocalTime = localTime;
        return output;
    }

    /// <summary>
    /// Called when an element has been reached
    /// </summary>
    /// <param name="eInfo"></param>
    private void AddElement(SceneElementInfo eInfo)
    {
        if (eInfo.element is TextEffect)
        {
            TextEffect effect = (TextEffect)eInfo.element;
            GameObject eObj = ceUI.InstantiateElement(eInfo, cutsceneTextPrefab, effect.cutsceneAnchor, effect.anchoredPosition);
            CutsceneTextUI ctUI = eObj.GetComponent<CutsceneTextUI>();
            ctUI.Init(effect, currentLocalTime, ProcessText(effect.text));
        }
    }

    /// <summary>
    /// Called when an element has ended
    /// </summary>
    /// <param name="eInfo"></param>
    private void RemoveElement(SceneElementInfo eInfo)
    {
        if (eInfo.element is TextEffect)
        {
            ceUI.DestroyElement(eInfo);
        }
        else if (eInfo.element is ZoomEffect)
        {
            ProcessZoom(eInfo);
        }
        else if (eInfo.element is BlackScreenEffect)
        {
            ProcessBlackScreen(eInfo);
        }
    }

    /// <summary>
    /// Called on every frame while progress is happening. Use currentLocalTime to get the local time
    /// </summary>
    /// <param name="eInfo"></param>
    private void ProcessElement(SceneElementInfo eInfo)
    {
        if (eInfo.element is ZoomEffect)
        {
            ProcessZoom(eInfo);
        }
        else if (eInfo.element is BlackScreenEffect)
        {
            ProcessBlackScreen(eInfo);
        }
    }

    private void ProcessZoom(SceneElementInfo eInfo)
    {
        SceneElement element = eInfo.element;
        ZoomEffect zoomInfo = (ZoomEffect)element;
        Vector3 initialLocation = currentScene.camPosition;
        float distance = Mathf.Lerp(zoomInfo.startDistance, zoomInfo.endDistance, // Start and end distances
            zoomInfo.zoomCurve.Evaluate(element.GetProgress(currentLocalTime))); // Element progress evaluated through the curve
        Vector3 finalLocation = initialLocation + cutsceneCamTransform.forward * distance;
        cutsceneCamTransform.position = finalLocation;
    }

    private void ProcessBlackScreen(SceneElementInfo eInfo)
    {
        SceneElement element = eInfo.element;
        BlackScreenEffect blackScreenInfo = (BlackScreenEffect)element;
        float alpha = Mathf.Clamp01(blackScreenInfo.fadeCurve.Evaluate(element.GetProgress(currentLocalTime)));
        blackScreen.SetAlpha(alpha, blackScreenInfo.screenColor);
    }

    private string ProcessText(string text)
    {
        string output = text;
        foreach (var kvp in currentScene.references)
        {
            output = output.Replace($"<{kvp.Key}>", srManager.GetRefString(kvp.Value));
        }
        return output;
    }
}
