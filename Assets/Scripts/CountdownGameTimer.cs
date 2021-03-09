using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class CountdownGameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    // for memory game only
    public TextMeshProUGUI secText;

    public int duration { get; set; }
    public GAMESTATES state;

    private void Start()
    {
        SetGameTimer(state);
        timerText.text = duration.ToString();
    }

    void SetGameTimer(GAMESTATES _state)
    {
        if (state == GAMESTATES.DETERMINED)
        {
            duration = TrinaxGlobal.Instance.ballGameDuration;
        }
        else if (state == GAMESTATES.KINDNESS)
        {
            duration = TrinaxGlobal.Instance.memoryGameDuration;
        }
        else if (state == GAMESTATES.ENERGETIC)
        {
            duration = TrinaxGlobal.Instance.runningGameDuration;
        }
    }

    public void Reset(int _duration)
    {
        duration = _duration;
        timerText.text = duration.ToString();
    }

    private void OnEnable()
    {
        CountdownTimer.OnCountdownFinished += OnCountdownFinished;
    }

    private void OnDisable()
    {
        CountdownTimer.OnCountdownFinished -= OnCountdownFinished;
    }

    void OnCountdownFinished()
    {
        StartCoroutine("Countdown");
    }

    private void Update()
    {
        if(state == GAMESTATES.ENERGETIC)
        {
            timerText.text = duration.ToString() + "sec";
        }
        else
        {
            timerText.text = duration.ToString()/* + " sec"*/;
        }

        if(duration <= 0 || TrinaxGlobal.Instance.IsGameOver)
        {
            StopCoroutine("Countdown");
        }
    }

    IEnumerator Countdown()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
            duration--;
        }
    }

}
