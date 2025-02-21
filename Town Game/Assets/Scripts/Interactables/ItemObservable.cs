using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ItemObservable : Observable
{
    public GameObject[] siObjects;
    public SubInteractable[] siStats;
    [Networked, Capacity(32)] public NetworkLinkedList<float> siProgress => default;
    [Networked] public int currentSI { get; set; }
    RunnerManager runnerManager;

    ChangeDetector changeDetector;

    [System.Serializable]
    public struct SubInteractable
    {
        public bool enabled; // Determines if this subinteractable can be changed, disable once certain animations are finished, like screws
        public bool hold; // Determines if the sub-interactable is hold or tap
        public float holdRate; // The rate of progress per second of sub-interactable being held
        public float cooldown; // Cooldowns for tap sub-interactables
    }

    private void Start()
    {
        runnerManager = FindFirstObjectByType<RunnerManager>();
    }

    public override void Spawned()
    {
        // Adds progress for each of the sub interactables
        if (!HasStateAuthority) return;
        for (int i = 0; i < siObjects.Length; i++)
        {
            siProgress.Add(0f);
        }
    }

    public void IncreaseSIProgress(float timeDelta, int siIndex)
    {
        SubInteractable currentStats = siStats[siIndex];
        if (!currentStats.enabled) return;
        float currentProgress = siProgress[siIndex];
        if (currentStats.hold)
        {
            siProgress.Set(siIndex, Mathf.Clamp01(currentProgress + timeDelta * siStats[siIndex].holdRate));
        }
        else
        {
            siProgress.Set(siIndex, 1f);
        }
    }

    /// <summary>
    /// For when the cursor is hovering over a subinteractable
    /// </summary>
    /// <param name="si"></param>
    public void ReceiveInteractable(GameObject si, bool firstFrame)
    {
        int siIndex = Array.IndexOf(siObjects, si);
        if (siIndex == -1) return;
        runnerManager.siPressed = siIndex;
        if (!runnerManager.heldOnSI && firstFrame)
        {
            runnerManager.heldOnSI = true;
        } // If held on button
        runnerManager.isHoldSI = siStats[siIndex].hold;
    }

    public void ResetInteractable()
    {
        runnerManager.siPressed = -1;
        runnerManager.heldOnSI = false;
    }
}
