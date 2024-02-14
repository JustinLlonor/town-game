using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public int gamePhase = 0;
    GameTimer gt;

    private void Awake()
    {
        gt = gameObject.GetComponent<GameTimer>();
    }
}
