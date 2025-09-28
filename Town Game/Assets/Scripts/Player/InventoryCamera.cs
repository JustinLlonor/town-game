using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCamera : MonoBehaviour
{
    public PlayerManager playerManager;
    public float camDistance = 1.5f;
    public float yOffset = 0.9f;
    private Player trackedPlayer;
    
    private void Awake()
    {
        playerManager.onInstantiatePlayer += OnInstantiate;
    }

    private void OnInstantiate(GameObject playerObject)
    {
        trackedPlayer = playerObject.GetComponent<Player>();
    }

    private void LateUpdate()
    {
        if (trackedPlayer == null) return;
        SetCamLoc();
    }

    private void SetCamLoc()
    {
        Quaternion lookRotation = Quaternion.AngleAxis(trackedPlayer.camDirection, Vector3.up);
        Vector3 lookVector = lookRotation * Vector3.forward * camDistance;
        Vector3 finalLoc = trackedPlayer.playerGFX.position + lookVector + Vector3.up * yOffset;
        Quaternion camLookRotation = Quaternion.AngleAxis(trackedPlayer.camDirection + 180f, Vector3.up);
        transform.position = finalLoc;
        transform.rotation = camLookRotation;
    }
}
