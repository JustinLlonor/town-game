using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Corpse : NetworkBehaviour//PunCallbacks
{
    public string nickname;
    public bool isCultist;
    public List<Evidence> evidence;
    public Rigidbody rootRb;

    UIManager ui;

    private void Awake()
    {
        ui = FindObjectOfType<UIManager>();
    }

    public void Init(PlayerEvidence playerEvidence, Vector3 velocity)
    {
        rootRb.velocity = velocity;
        //playerEvidence.ApplyEvidence(gameObject);
    }

    //[PunRPC]
    public void AddEvidence(string[] icons, string[] descriptions, float time)
    {
        evidence.Add(new Evidence(icons, descriptions, time));
    }

    //[PunRPC]
    //public void SetCorpseData(Photon.Realtime.Player player)
    //{
    //    nickname = (string)player.CustomProperties["name"];
    //    isCultist = (bool)player.CustomProperties["isCultist"];
    //    PlayerClothing pc = transform.GetComponent<PlayerClothing>();
    //    pc.isMale = (bool)player.CustomProperties["isMale"];
    //}

    public void InspectCorpse()
    {
        ui.OpenCorpse(evidence, nickname, isCultist);
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
