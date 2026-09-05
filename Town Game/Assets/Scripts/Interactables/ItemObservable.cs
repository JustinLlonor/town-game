using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemObservable : Observable
{
    public GameObject[] siObjects;
    public SubInteractable[] siStats;
    [Networked, Capacity(32)] public NetworkLinkedList<float> siProgress => default;
    [Networked, Capacity(32)] public NetworkLinkedList<TickTimer> cooldownTimers => default;
    List<float> inputAuthorityProgress = new List<float>();
    [Networked] public int currentSI { get; set; }
    public float[] previousProgress = new float[0];
    RunnerManager runnerManager;
    bool init = false;
    PlayerRef previousInputAuthority;

    ChangeDetector changeDetector;

    public delegate void SIEvent(int si, float progress);
    public SIEvent onSIUpdate; // When a sub-interactable updates its progress

    [System.Serializable]
    public struct SubInteractable
    {
        public bool enabled; // Determines if this subinteractable can be changed, disable once certain animations are finished, like screws
        public bool hold; // Determines if the sub-interactable is hold or tap
        public bool onCooldown;
        // Stats below should be unchanged
        public float holdRate; // The rate of progress per second of sub-interactable being held
        public float cooldown; // Cooldowns for tap sub-interactables
    }

    private void Start()
    {
        previousProgress = new float[siObjects.Length];
        runnerManager = FindFirstObjectByType<RunnerManager>();
    }

    public override void Render()
    {
        ChangeDetectInputAuthority();
        InputAuthorityChange();
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(siProgress):
                    if (!Runner.IsResimulation) ProgressChangeDetection();
                    break;
                case nameof(currentPlayer):
                    if (currentPlayer == PlayerRef.None)
                    {
                        for (int i = 0; i < siStats.Length; i++)
                        {
                            siStats[i].onCooldown = false;
                        }
                    }
                    break;
            }
        }
    }

    private void InputAuthorityChange()
    {
        if (previousInputAuthority != Object.InputAuthority)
        {
            if (Object.InputAuthority == Runner.LocalPlayer)
            {
                inputAuthorityProgress.Clear();
                for (int i = 0; i < siProgress.Count; i++)
                {
                    inputAuthorityProgress.Add(siProgress[i]);
                }
            }
            previousInputAuthority = Object.InputAuthority;
        }
    }

    private void ProgressChangeDetection()
    {
        if (!init) return;
        for (int i = 0; i < siProgress.Count; i++)
        {
            if (siProgress[i] != previousProgress[i]) // If they are not equal, then invoke update event and make them equal
            {
                if (!HasInputAuthority) onSIUpdate.Invoke(i, siProgress[i]);
                previousProgress[i] = siProgress[i];
            }
        }
    }

    private void ChangeDetectInputAuthority()
    {
        if (!HasInputAuthority) return;
        for (int i = 0; i < inputAuthorityProgress.Count; i++)
        {
            if (inputAuthorityProgress[i] != previousProgress[i]) // If they are not equal, then invoke update event and make them equal
            {
                onSIUpdate.Invoke(i, inputAuthorityProgress[i]);
                previousProgress[i] = inputAuthorityProgress[i];
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        for (int i = 0; i < cooldownTimers.Count; i++)
        {
            if (cooldownTimers[i].IsRunning)
            {
                if (cooldownTimers[i].Expired(Runner))
                {
                    siStats[i].onCooldown = false;
                    siProgress.Set(i, 0f);
                    inputAuthorityProgress[i] = 0f;
                    cooldownTimers.Set(i, TickTimer.None);
                }
            }
        }
    }

    public override void Spawned()
    {
        init = true;
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        // Adds progress for each of the sub interactables
        if (!HasStateAuthority) return;
        for (int i = 0; i < siObjects.Length; i++)
        {
            siProgress.Add(0f);
            inputAuthorityProgress.Add(0f);
            cooldownTimers.Add(TickTimer.None);
        }
        previousInputAuthority = PlayerRef.None;
    }

    public void IncreaseSIProgress(float timeDelta, int siIndex)
    {
        SubInteractable currentStats = siStats[siIndex];
        if (!currentStats.enabled) return;
        if (currentStats.onCooldown) return;
        float currentProgress = siProgress[siIndex];
        if (currentStats.hold)
        {
            float newHold = Mathf.Clamp01(currentProgress + timeDelta * siStats[siIndex].holdRate);
            siProgress.Set(siIndex, newHold);
            inputAuthorityProgress[siIndex] = newHold;
        }
        else
        {
            siProgress.Set(siIndex, 1f);
            inputAuthorityProgress[siIndex] = 1f;
            AddCooldown(currentStats.cooldown, siIndex);
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

    private void AddCooldown(float cooldownLength, int siIndex)
    {
        siStats[siIndex].onCooldown = true;
        cooldownTimers.Set(siIndex, TickTimer.CreateFromSeconds(Runner, cooldownLength));
    }

    // For instantaneous button signalling
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendButtonUpdate(int siIndex, PlayerRef messageSource)
    {
        onSIUpdate.Invoke(siIndex, 1f);
    }
}
