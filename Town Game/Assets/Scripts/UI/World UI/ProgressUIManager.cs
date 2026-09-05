using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressUIManager : MonoBehaviour
{
    public GameObject progressUIPrefab;
    public WorldUIManager worldUIManager;
    public float fadeTime = .25f;
    private Dictionary<ProgressHandler, int> handlerIds = new Dictionary<ProgressHandler, int>();
    // Increases to destroyTime while looking, decreases while not looking
    private Dictionary<string, float> destroyTimers = new Dictionary<string, float>();
    private string currentHandler;
    private int idCounter = 0;

    private void Update()
    {
        MaintainDestroyTimers();
        MaintainHandlerData();
    }

    private void MaintainHandlerData()
    {
        foreach (ProgressHandler handler in handlerIds.Keys)
        {
            string handlerId = $"prh{handlerIds[handler]}";
            WorldUI foundUI = worldUIManager.GetWorldUI(handlerId);
            if (foundUI == null) continue;
            foundUI.doFollow = (currentHandler == handlerId); // Follow only if looking
            EntryPointUI epUI = foundUI.gameObject.GetComponent<EntryPointUI>();
            epUI.SetHandlerInfo(handler);
            AlphaGroup alphaGroup = foundUI.gameObject.GetComponent<AlphaGroup>();
            float percent = destroyTimers[handlerId] / fadeTime;
            alphaGroup.alpha = percent;
        }
    }

    private void MaintainDestroyTimers()
    {
        List<string> timerIds = new List<string>(destroyTimers.Keys);
        foreach (string id in timerIds)
        {
            if (id == currentHandler)
            {
                destroyTimers[id] += Time.deltaTime;
                if (destroyTimers[id] > fadeTime) destroyTimers[id] = fadeTime;
                continue;
            }
            destroyTimers[id] -= Time.deltaTime; 
            // Do animation here
            if (destroyTimers[id] < 0f)
            {
                worldUIManager.RemoveWorldUI(id);
                destroyTimers.Remove(id);
            }
        }
    }

    public void SendProgressInfo(ProgressHandler handler, Vector3 hitLoc = new Vector3())
    {
        if (handler == null)
        {
            currentHandler = null;
            return;
        }
        AssignId(handler);
        string newId = $"prh{handlerIds[handler]}";
        // Add a destroy timer for this handler if it doesn't exist
        if (!destroyTimers.ContainsKey(newId))
        {
            destroyTimers.Add(newId, 0f);
        }
        currentHandler = newId;
        // Create if it doesn't exist
        if (!worldUIManager.WorldUIExists(newId))
        {
            WorldUI wUI = worldUIManager.CreateWorldUI(newId, progressUIPrefab, hitLoc);
            wUI.gameObject.GetComponent<EntryPointUI>().SetHandlerInfo(handler, true);
            return;
        }
        // if it does, update the locaiton
        worldUIManager.SetWorldUITarget(newId, hitLoc);
    }

    private void AssignId(ProgressHandler handler)
    {
        if (!handlerIds.ContainsKey(handler))
        {
            handlerIds.Add(handler, idCounter++);
        }
    }
}
