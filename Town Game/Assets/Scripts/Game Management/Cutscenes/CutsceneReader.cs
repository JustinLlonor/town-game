using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneReader : MonoBehaviour
{
    public CutsceneManager cutsceneManager;
    CameraManager cameraManager;

    private void Awake()
    {
        cameraManager = FindAnyObjectByType<CameraManager>();
    }

    public void StartReading()
    {

    }

    public void StopReading()
    {

    }

    public void ReadCutscene(float progress)
    {

    }
}
