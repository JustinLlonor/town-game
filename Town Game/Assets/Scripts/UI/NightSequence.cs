//using Photon.Pun;
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
    bool nightScreen = false;

    private void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
        bs = FindFirstObjectByType<BlackScreen>();
        pm = FindFirstObjectByType<PlayerManager>();
        rm = FindFirstObjectByType<RoomManager>();

        gm.OnNightSkipStart += AllowNightScreen;
        gm.OnDayStart += DayStuff;
        pm.onInstantiatePlayer += GetReferences;
    }

    private void AllowNightScreen()
    {
        nightScreen = true;
    }

    private void Update()
    {
        if (!gm.init) return;
        if ((!gm.skippedNight) || (!nightScreen)) return;
        if (gm.nightTimer.RemainingTime(gm.Runner) <= 2.75f)
        {
            nightScreen = false;
            NightStuff();
        }
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
        //Transform tpTransform = rm.playerRooms[(int)PhotonNetwork.LocalPlayer.CustomProperties["room"]].spawnTransform;
        //if (teleport) pm.Teleport(tpTransform.position, tpTransform.rotation);
        yield return new WaitForSeconds(1f);
        if (blackScreen) bs.StartAlphaTransition(0f, 2.5f);
        yield return new WaitForSeconds(4.1f);
        newNightUI.SetActive(false);
    }

    void SetCultistText()
    {
        int cultistsLeft = gm.cultistsLeft;
        if (cultistsLeft == 1)
        {
            cultistText.text = cultistsLeft + " cultist remains.";
            return;
        }
        cultistText.text = cultistsLeft + " cultists remain.";
    }
}
