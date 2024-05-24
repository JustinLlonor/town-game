using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NightSequence : MonoBehaviour
{
    public GameObject newNightUI;
    public TextMeshProUGUI nightText;
    public TextMeshProUGUI cultistText;
    GameManager gm;
    BlackScreen bs;
    PlayerManager pm;
    PlayerRoom pr;
    RoomManager rm;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        bs = FindObjectOfType<BlackScreen>();
        pm = FindObjectOfType<PlayerManager>();
        rm = FindObjectOfType<RoomManager>();

        gm.OnNightSkip += NightStuff;
        pm.OnInstantiatePlayer += GetReferences;
    }

    void GetReferences(GameObject player)
    {
        pr = player.GetComponent<PlayerRoom>();
        Debug.Log(pr);
    }

    void NightStuff()
    {
        bs.HideTexts();
        bs.SetAlpha(0);
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        nightText.text = "Night " + (gm.currentDay + 1);
        SetCultistText();
        bs.StartAlphaTransition(1f, 2.5f);
        yield return new WaitForSeconds(3f);
        newNightUI.SetActive(true);
        Transform tpTransform = rm.playerRooms[(int)PhotonNetwork.LocalPlayer.CustomProperties["room"]].spawnTransform;
        pm.Teleport(tpTransform.position, tpTransform.rotation);
        yield return new WaitForSeconds(1f);
        bs.StartAlphaTransition(0f, 2.5f);
        yield return new WaitForSeconds(4.1f);
        newNightUI.SetActive(false);
    }

    void SetCultistText()
    {
        if (gm.cultists.Length == 1)
        {
            cultistText.text = gm.cultists.Length + " cultist remains.";
            return;
        }
        cultistText.text = gm.cultists.Length + " cultists remain.";
    }
}
