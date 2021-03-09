using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    [HideInInspector]
    public float currentTime = 0;

    //string minutes;
    //string seconds;
    //string milliSeconds;

    bool startGameTimer = false;

    private void Start()
    {
        CountdownTimer.OnCountdownFinished += OnCountdownFinished;
        timerText.text = "00:00";
    }

    void OnCountdownFinished()
    {
        startGameTimer = true;
        currentTime = TrinaxGlobal.Instance.runningGameDuration;
    }

    private void Update()
    {
        if (TrinaxGlobal.Instance.IsGameOver)
            return;

        if (startGameTimer)
        {
            StartTimer();
        }

    }

    public void StartTimer()
    {
        if (currentTime <= 0)
        {
            timerText.text = "00:00";
        }
        else
        {
            timerText.text = TrinaxGlobal.Instance.FloatToTime(currentTime, "00.00");
        }
        //currentTime = (Time.time - startTime) * 0.5f;
        currentTime -= Time.deltaTime * 0.5f;
    }

    public void ResetGameTimer()
    {
        startGameTimer = false;
        currentTime = 0;
        timerText.text = TrinaxGlobal.Instance.FloatToTime(TrinaxGlobal.Instance.runningGameDuration, "00.00");
    }

}
