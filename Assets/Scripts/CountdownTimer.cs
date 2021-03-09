using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public static System.Action OnCountdownFinished;

    TextMeshProUGUI countdownText;
    Animator anim;

    public Image target;
    public Sprite[] images;

    public int timeTostart = 3;

    private void OnEnable()
    {
        anim = GetComponent<Animator>();
        //countdownText = GetComponent<TextMeshProUGUI>();
        //countdownText.text = timeTostart.ToString();
        //StartCoroutine(StartCountDown());
        target.sprite = images[2];
    }

   public void SetCountAndStart()
    {
        StartCoroutine(StartCountDown());
    }

    IEnumerator StartCountDown()
    {
        int count = timeTostart;

        for (int i = 0; i < count; i++)
        {
            //countdownText.text = (count - i).ToString();


            if (anim != null)
                anim.SetTrigger(i == 2 ? "Exit" : "Pulse");

            if(i == 0)
            {
                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.COUNT3);
                target.sprite = images[2];
            }
            else if(i == 1)
            {
                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.COUNT2);
                target.sprite = images[1];
            }
            else if(i == 2)
            {
                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.COUNT1);
                target.sprite = images[0];
            }
            yield return new WaitForSeconds(1);

        }

        if (OnCountdownFinished != null)
            // event send to gameManager
            OnCountdownFinished(); 

    }

    public void DeactivateSelf()
    {
        gameObject.SetActive(false);
    }

}
