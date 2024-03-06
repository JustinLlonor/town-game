using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInfoView : MonoBehaviourPunCallbacks
{
    public Gradient healthGradient = new Gradient();
    public string[] healthTextGradient = new string[] { };
    public PlayerStats stats;
    public PhotonView view;
    Interactable vi;
    int previousIndex = -1;

    private void Awake()
    {
        vi = transform.GetComponent<Interactable>();
    }

    private void Start()
    {
        view.RPC("SetNickname", RpcTarget.OthersBuffered, SessionData.nickname);
        if (view.IsMine)
        {
            transform.GetComponent<BoxCollider>().enabled = false;
        }
    }

    private void Update()
    {
        UpdateHP(Mathf.Clamp01(stats.HP / stats.maxHP));
    }

    [PunRPC]
    public void SetNickname(string newName)
    {
        vi.hovers[0].lore = newName;
        vi.canInteract = true;
    }

    void UpdateHP(float eval)
    {
        if (view.IsMine) return;
        int newIndex = Mathf.FloorToInt(healthTextGradient.Length*(1-eval));
        if (newIndex != previousIndex)
        {
            previousIndex = newIndex;
            vi.hovers[1].lore = healthTextGradient[newIndex];
        }
        vi.hovers[1].color = healthGradient.Evaluate(1-eval);
    }
}
