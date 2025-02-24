using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
//using Photon.Pun;
//using Photon.Realtime;
//using WebSocketSharp;

public class PlayerInfoView : NetworkBehaviour
{
    public Gradient healthGradient = new Gradient();
    public string[] healthTextGradient = new string[] { };
    public Gradient sanityGradient = new Gradient();
    public string[] sanityTextGradient = new string[] { };
    [HideInInspector] public PlayerStats stats;
    [HideInInspector] public Player player;
    public float recievedHP = 100f;
    Interactable vi;
    [HideInInspector] public NetworkObject no;
    int previousIndex = -1;
    bool updatedNick = false;
    bool init = false;

    private void Awake()
    {
        vi = transform.GetComponent<Interactable>();
    }

    private void Start()
    {
        //if (!view.IsMine) return;
        if (no.HasInputAuthority)
        {
            transform.GetComponent<Collider>().enabled = false;
        }
    }

    public override void Spawned()
    {
        init = true;
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
        if (!init) return;
        if (updatedNick) return;
        if (!player.nickname.IsNullOrEmpty())
        {
            SetNickname(player.nickname);
            updatedNick = true;
        }
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
