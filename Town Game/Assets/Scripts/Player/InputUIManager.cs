using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class InputUIManager : MonoBehaviour
{
    public bool disableOnUI = true;
    PlayerInput playerInput;

    private void Start()
    {
        playerInput = gameObject.GetComponent<PlayerInput>();
        if (gameObject.GetComponent<PhotonView>() != null)
        {
            if (!gameObject.GetComponent<PhotonView>().IsMine) return;
        }
        UIManager.instance.OnUIOpen += DisableInputs;
        UIManager.instance.OnUIClose += EnableInputs;
    }

    public void EnableInputs()
    {
        playerInput.enabled = disableOnUI;
    }

    public void DisableInputs()
    {
        playerInput.enabled = !disableOnUI;
    }
}
