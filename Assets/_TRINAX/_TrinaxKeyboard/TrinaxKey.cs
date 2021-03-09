using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TrinaxKey : MonoBehaviour {

    public KeyCode keycode;
    public string custom = "";

    int rnd = 0;

    private void Start() {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(()=> {
            rnd = Random.Range(0, 3);
            //if (rnd == 0) {
            //    TrinaxAudioManager.Instance.PlaySound(TrinaxAudioManager.Clips.TYPING);
            //} else if (rnd == 1) {
            //    TrinaxAudioManager.Instance.PlaySound(TrinaxAudioManager.Clips.TYPING_2);
            //} else {
            //    TrinaxAudioManager.Instance.PlaySound(TrinaxAudioManager.Clips.TYPING_3);
            //}

            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.TYPING);
            
            transform.DOScale(Vector2.one * 0.8f, 0.000001f).OnComplete(() => {
                transform.DOScale(Vector2.one, 0.2f).SetEase(Ease.OutBack);
            });

            this.GetComponentInParent<TrinaxOnScreenKB>().OnKeyDown(keycode, custom);
            //TrinaxOnScreenKB.Instance.OnKeyDown(keycode, custom);
        });
    }
}
