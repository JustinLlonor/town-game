using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI text;

    private void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        if (text == null) return;
        text.text = $"{PhotonNetwork.LocalPlayer.CustomProperties["money"]}";
    }
}
