using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class MinimapRoomText : MonoBehaviour
{
    public PlayerRoom trackedPM;
    TextMeshProUGUI text;
    Animator animator;

    private void Awake()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
        animator = gameObject.GetComponent<Animator>();
        PlayerManager pm = FindFirstObjectByType<PlayerManager>();
        pm.onInstantiatePlayer += AddReferences;
        if (trackedPM != null) trackedPM.OnEnterRoom += PlayRoomText;
    }

    void AddReferences(GameObject player)
    {
        trackedPM = player.GetComponent<Player>().playerRoom;
        trackedPM.OnEnterRoom += PlayRoomText;
    }

    void PlayRoomText(MapRoom room)
    {
        text.text = room.roomName;
        animator.Play("RoomPopup", 0, 0f);
    }
}
