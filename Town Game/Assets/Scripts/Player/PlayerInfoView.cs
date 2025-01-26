using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
//using Photon.Realtime;
//using WebSocketSharp;

public class PlayerInfoView : NetworkBehaviour
{
    public Gradient healthGradient = new Gradient();
    public string[] healthTextGradient = new string[] { };
    public Gradient sanityGradient = new Gradient();
    public string[] sanityTextGradient = new string[] { };
    public PlayerStats stats;
    public float recievedHP = 100f;
    Interactable vi;
    [HideInInspector] public NetworkObject no;
    int previousIndex = -1;
    bool updatedNick = false;

    private void Awake()
    {
        vi = transform.GetComponent<Interactable>();
    }

    private void Start()
    {
        //if (!view.IsMine) return;
        if (no.HasInputAuthority)
        {
            transform.GetComponent<BoxCollider>().enabled = false;
        }
    }
   
    private void Update()
    {
        if (vi == null) return;
        UpdateNickname();
        UpdateHP(Mathf.Clamp01(recievedHP / stats.maxHP));
    }

    public void SetNickname(string newName)
    {
        vi.hovers[0].lore = newName;
        vi.canInteract = true;
    }

    void UpdateNickname()
    {
        if (updatedNick) return;
        //if (view.Owner == null) return;
        //if (!((string)view.Owner.CustomProperties["name"]).IsNullOrEmpty())
        //{
        //    view.RPC("SetNickname", RpcTarget.OthersBuffered, (string)view.Owner.CustomProperties["name"]);
        //    updatedNick = true;
        //}
    }

    void UpdateHP(float eval)
    {
        //if (view.IsMine) return;
        int newIndex = Mathf.FloorToInt(healthTextGradient.Length*(1-eval));
        if (newIndex != previousIndex)
        {
            previousIndex = newIndex;
            vi.hovers[1].lore = healthTextGradient[newIndex];
        }
        vi.hovers[1].color = healthGradient.Evaluate(1-eval);
    }
}
