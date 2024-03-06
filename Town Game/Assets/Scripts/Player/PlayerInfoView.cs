using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

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
        if (!view.IsMine) return;
        string nickname = SessionData.nickname;
        int copyIndex = 1;
        int i = 0;
        while (i < PhotonNetwork.PlayerList.Length)
        {
            Player player = PhotonNetwork.PlayerList[i];
            if ((string)player.CustomProperties["name"] == nickname && (player != PhotonNetwork.LocalPlayer))
            {
                copyIndex++;
                nickname = SessionData.nickname + " " + copyIndex;
                i = 0;
                continue;
            }
            i++;
        }
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties["name"] = nickname;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        view.RPC("SetNickname", RpcTarget.OthersBuffered, nickname);
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
