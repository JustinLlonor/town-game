using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ProgressManager : NetworkBehaviour
{
    [Networked, Capacity(100)] public NetworkLinkedList<NetworkId> progressObjects => default;
    private List<NetworkId> processedObjects = new List<NetworkId>();
    private Dictionary<NetworkId, GameObject> objectIds = new Dictionary<NetworkId, GameObject>();
    private Dictionary<GameObject, NetworkId> idObjects = new Dictionary<GameObject, NetworkId>();
    // Progress handler objects found on the client
    private Dictionary<GameObject, ProgressHandler> objectHandlers = new Dictionary<GameObject, ProgressHandler>();
    private ChangeDetector changeDetector;

    public void AddHandler(ProgressHandler handler)
    {
        progressObjects.Add(handler.GetComponent<NetworkObject>().Id);
    }

    /// <summary>
    /// Gets the handler object from the network id. Returns null if not found
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GameObject GetHandlerObject(NetworkId id)
    {
        if (!objectIds.ContainsKey(id)) return null;
        return objectIds[id];
    }

    public NetworkId GetHandlerId(GameObject gameObject)
    {
        return idObjects[gameObject];
    }

    /// <summary>
    /// Gets the progress handler from the game object. Returns null if not found
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <returns></returns>
    public ProgressHandler GetProgressHandler(GameObject gameObject)
    {
        if (!objectHandlers.ContainsKey(gameObject)) return null;
        return objectHandlers[gameObject];
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        UpdateProgressList();
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(progressObjects):
                    UpdateProgressList();
                    break;
            }
        }
    }

    private void UpdateProgressList()
    {
        foreach (NetworkId id in progressObjects)
        {
            if (processedObjects.Contains(id)) continue;
            NetworkObject foundNObject;
            if (!Runner.TryFindObject(id, out foundNObject)) return;
            GameObject foundObject = foundNObject.gameObject;
            ProgressHandler foundHandler = foundObject.GetComponent<ProgressHandler>();
            objectIds.Add(id, foundObject);
            idObjects.Add(foundObject, id);
            objectHandlers.Add(foundObject, foundHandler);
            // Add to processed objects so it's not found on the client again
            processedObjects.Add(id);
        }
    }
}
