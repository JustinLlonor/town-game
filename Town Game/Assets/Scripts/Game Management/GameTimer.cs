using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using TMPro;

public class GameTimer : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isTicking = false;
    public UnityEvent onTimerStart;
    public UnityEvent onTimerFinish;
    public UnityEvent onTimerStop;
    public TextMeshProUGUI timerText;
    public float gameTimer;
    public float clientTimer;
    PhotonView view;

    public void StartTimer(float time)
    {
        gameTimer = time;
        onTimerStart.Invoke();
        isTicking = true;
        clientTimer = gameTimer;
    }

    public void StopTimer()
    {
        onTimerStop.Invoke();
        isTicking = false;
    }

    public void ShowTimerText()
    {
        timerText.gameObject.SetActive(true);
    }

    public void HideTimerText()
    {
        timerText.gameObject.SetActive(false);
    }

    void Awake()
    {
        view = gameObject.GetComponent<PhotonView>();
    }

    void Update()
    {
        ClientTimer();
        TimerUI();
        if (!PhotonNetwork.IsMasterClient) return;
        Timer();
    }

    void TimerUI()
    {
        if (timerText == null) return;
        timerText.text = Mathf.Ceil(clientTimer).ToString();
    }

    void Timer()
    {
        if (!isTicking) return;
        gameTimer -= Time.deltaTime;
        clientTimer = gameTimer;
        if (gameTimer < 0f)
        {
            FinishTimer();
        }
    }

    void FinishTimer()
    {
        onTimerFinish.Invoke();
        isTicking = false;
    }

    void ClientTimer()
    {
        if (PhotonNetwork.IsMasterClient) return;
        if (!isTicking) return;

        if (clientTimer > gameTimer)
        {
            clientTimer = gameTimer;
        }
        if (gameTimer - clientTimer > .5f)
        {
            clientTimer = gameTimer;
        }

        clientTimer -= Time.deltaTime;
        if (clientTimer < 0f) clientTimer = -0.1f;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameTimer);
            stream.SendNext(isTicking);
        }
        else
        {
            gameTimer = (float)stream.ReceiveNext();
            isTicking = (bool)stream.ReceiveNext();
        }
    }
}
