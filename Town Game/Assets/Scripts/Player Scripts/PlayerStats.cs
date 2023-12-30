using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerStats : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] float maxHP = 100f;
    [SerializeField] float HP = 100f;
    [SerializeField] float HPRegenSpeed = 5f;
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float stamina = 100f;
    [SerializeField] float staminaRegenSpeed = 15f;



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
