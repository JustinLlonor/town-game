using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerEvidence : NetworkBehaviour//PunCallbacks
{
    public Dictionary<string, Evidence> evidence = new Dictionary<string, Evidence>();

    /// <summary>
    /// Adds evidence to a player, should only be called on the host/server
    /// </summary>
    /// <param name="name"></param>
    /// <param name="icons"></param>
    /// <param name="descriptions"></param>
    /// <param name="time"></param>
    public void AddEvidence(string name, string[] icons, string[] descriptions, float time = 0f)
    {
        if (evidence.ContainsKey(name))
        {
            evidence[name] = new Evidence(icons, descriptions, time);
            return;
        }
        evidence.Add(name, new Evidence(icons, descriptions, time));
    }

    public void ApplyEvidence(GameObject corpse)
    {
        Corpse c = corpse.GetComponent<Corpse>();
        foreach (KeyValuePair<string, Evidence> p in evidence)
        {
            Evidence e = evidence[p.Key];
            c.AddEvidence(e.icons, e.descriptions, e.time);
        }
    }
}
