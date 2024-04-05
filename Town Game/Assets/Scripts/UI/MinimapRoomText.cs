using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MinimapRoomText : MonoBehaviour
{
    public PlayerRoom trackedPM;
    TextMeshProUGUI text;
    Animator animator;

    private void Awake()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
        animator = gameObject.GetComponent<Animator>();
        PlayerManager pm = FindObjectOfType<PlayerManager>();
        pm.OnInstantiatePlayer += AddReferences;
        if (trackedPM != null) trackedPM.OnEnterRoom += PlayRoomText;
    }

    void AddReferences(GameObject player)
    {
        trackedPM = player.GetComponent<PlayerRoom>();
        trackedPM.OnEnterRoom += PlayRoomText;
    }

    void PlayRoomText(Room room)
    {
        text.text = room.roomName;
        animator.Play("RoomPopup", 0, 0f);
    }
}
