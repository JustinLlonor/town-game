using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Corpse : NetworkBehaviour//PunCallbacks
{
    public string nickname { get; set; }
    public bool isCultist { get; set; }
    public Rigidbody rootRb;
    //[Networked, Capacity(0)] public NetworkLinkedList<Evidence> evidences { get; }

    UIManager ui;

    private void Awake()
    {
        ui = FindFirstObjectByType<UIManager>();
    }

    public void Init(Vector3 velocity, bool isMale)
    {
        rootRb.velocity = velocity;
        GetComponent<PlayerClothing>().isMale = isMale;
    }

    public void AddEvidence(string[] icons, string[] descriptions, float time)
    {
        //evidence.Add(new Evidence(icons, descriptions, time));
    }

    //[PunRPC]
    //public void SetCorpseData(Photon.Realtime.Player player)
    //{
    //    nickname = (string)player.CustomProperties["name"];
    //    isCultist = (bool)player.CustomProperties["isCultist"];
    //    PlayerClothing pc = transform.GetComponent<PlayerClothing>();
    //    pc.isMale = (bool)player.CustomProperties["isMale"];
    //}

    public void InspectCorpse() // Make corpses ask for information from the server, and make locations interest
    {
        return;
        //ui.OpenCorpse(evidence, nickname, isCultist);
    }

    public void SetVelocity(Vector3 velocity)
    {
        Rigidbody[] limbs = transform.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody limb in limbs)
        {
            limb.velocity = velocity;
            limb.angularVelocity = velocity;
        }
    }
}
