using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;

public class Corpse : MonoBehaviour//PunCallbacks
{
    public string nickname;
    public bool isCultist;
    public List<Evidence> evidence;

    UIManager ui;

    private void Awake()
    {
        ui = FindObjectOfType<UIManager>();
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
