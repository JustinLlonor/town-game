using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerEvidence : MonoBehaviourPunCallbacks
{
    public Dictionary<string, Evidence> evidence = new Dictionary<string, Evidence>();

    [PunRPC]
    public void AddEvidence(string name, string[] icons, string[] descriptions, float time = 0f)
    {
        if (evidence.ContainsKey(name))
        {
            evidence[name].icons = icons;
            evidence[name].descriptions = descriptions;
            evidence[name].time = time;
            return;
        }
        evidence.Add(name, new Evidence(icons, descriptions, time));
    }

    public void ApplyEvidence(GameObject corpse)
    {
        PhotonView view = corpse.GetComponent<PhotonView>();
        foreach (KeyValuePair<string, Evidence> p in evidence)
        {
            Evidence e = evidence[p.Key];
            view.RPC("AddEvidence", RpcTarget.All, e.icons, e.descriptions, e.time);
        }
    }
}
