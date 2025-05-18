using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMinimap : MonoBehaviour
{
    public Rigidbody trackedRigidbody;
    RunnerManager runnerManager;
    Minimap minimap;

    private void Awake()
    {
        PlayerManager pm = FindFirstObjectByType<PlayerManager>();
        minimap = GetComponent<Minimap>();
        runnerManager = FindAnyObjectByType<RunnerManager>();
        pm.OnInstantiatePlayer += AddReferences;
    }

    private void AddReferences(GameObject playerObject)
    {
        trackedRigidbody = playerObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (trackedRigidbody == null) return;
        minimap.SetPosition(trackedRigidbody.position);
        minimap.SetRotation(runnerManager.orientation);
    }
}
