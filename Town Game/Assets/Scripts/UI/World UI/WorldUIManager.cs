using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUIManager : MonoBehaviour
{
    private Dictionary<string, WorldUI> worldUIs = new Dictionary<string, WorldUI>();

    public WorldUI CreateWorldUI(string id, GameObject prefab, Vector3 startPos)
    {
        if (worldUIs.ContainsKey(id)) return worldUIs[id];
        GameObject uiObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        WorldUI wUI = uiObject.GetComponent<WorldUI>();
        wUI.SetPosition(startPos);
        worldUIs.Add(id, wUI);
        return wUI;
    }

    public void SetWorldUITarget(string id, Vector3 target)
    {
        if (!worldUIs.ContainsKey(id)) return;
        worldUIs[id].SetTarget(target);
    }

    public void RemoveWorldUI(string id)
    {
        if (!worldUIs.ContainsKey(id)) return;
        Destroy(worldUIs[id].gameObject);
        worldUIs.Remove(id);
    }

    public WorldUI GetWorldUI(string id)
    {
        if (!worldUIs.ContainsKey(id)) return null;
        return worldUIs[id];
    }

    public bool WorldUIExists(string id)
    {
        return worldUIs.ContainsKey(id); // if contians key, then returns true
    }
}
