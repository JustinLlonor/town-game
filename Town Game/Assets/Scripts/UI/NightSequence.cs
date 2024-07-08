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

        //gm.OnNightSkip += NightStuff;
        gm.OnDayStart += DayStuff;
        pm.OnInstantiatePlayer += GetReferences;

    }

    void GetReferences(GameObject player)
    {
        pr = player.GetComponent<PlayerRoom>();
        Debug.Log(pr);
    }

    void DayStuff()
    {
        bs.HideTexts();
        bs.SetAlpha(0);
        StartCoroutine(Sequence("Day ", false, false, false));
    }

    void NightStuff()
    {
        bs.HideTexts();
        bs.SetAlpha(0);
        StartCoroutine(Sequence("Night "));
    }

    IEnumerator Sequence(string cycleText, bool teleport = true, bool blackScreen = true, bool waitForTransition = true)
    {
        nightText.text = cycleText + (gm.currentDay + 1);
        SetCultistText();
        if (blackScreen) bs.StartAlphaTransition(1f, 2.5f);
        if (waitForTransition) yield return new WaitForSeconds(3f);
        newNightUI.SetActive(true);
        Transform tpTransform = rm.playerRooms[(int)PhotonNetwork.LocalPlayer.CustomProperties["room"]].spawnTransform;
        if (teleport) pm.Teleport(tpTransform.position, tpTransform.rotation);
        yield return new WaitForSeconds(1f);
        if (blackScreen) bs.StartAlphaTransition(0f, 2.5f);
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
