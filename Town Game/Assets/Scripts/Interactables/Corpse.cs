using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Corpse : MonoBehaviourPunCallbacks
{
    public List<Evidence> evidence;

    UIManager ui;

    private void Awake()
    {
        ui = FindObjectOfType<UIManager>();
    }

    [PunRPC]
    public void AddEvidence(string[] icons, string[] descriptions, float time)
    {
        evidence.Add(new Evidence(icons, descriptions, time));
    }

    public void InspectCorpse()
    {
        ui.OpenCorpse(evidence);
    }
}
