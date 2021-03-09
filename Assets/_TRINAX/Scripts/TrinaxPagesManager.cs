using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//public enum PAGES
//{
//    PAGE1,
//    PAGE2, 
//    PAGE3,
//}

public class TrinaxPagesManager : MonoBehaviour {

    public GameObject[] pages;
    CanvasGroup[] pagesCanvas;

    public bool IsReady { get; set; }
    bool isTweening;
    Tweener hideTweener;
    Tweener showTweener;

    void Start()
    {

    }

    public void Init()
    {
        IsReady = false;
        isTweening = false;

        pagesCanvas = new CanvasGroup[pages.Length];
        for (int i = 0; i < pagesCanvas.Length; ++i)
        {
            pagesCanvas[i] = pages[i].GetComponent<CanvasGroup>();
            pagesCanvas[i].alpha = 0;
            pages[i].SetActive(false);
        }

        HideAllPages(true);
        IsReady = true;
    }

    void Update ()
    {
		
	}

    void HideAllPages(bool hideTransition) {
        for (int i = 0; i < pages.Length; ++i) {
            int index = i;

            if (pages[index].activeSelf) {
                if (hideTransition) {
                    pages[index].transform.localScale = Vector3.one;
                    //pagesCanvas[index].blocksRaycasts = true;
                    pagesCanvas[index].alpha = 0.001f;
                    pages[index].SetActive(false);
                } else {
                    //pagesCanvas[i].blocksRaycasts = false;
                    hideTweener = pagesCanvas[i].DOFade(0.001f, 0.5f).OnComplete(() => {
                        pages[index].transform.localScale = Vector3.one;
                        //pagesCanvas[index].blocksRaycasts = true;
                        pages[index].SetActive(false);
                    });
                    //pages[i].transform.DOScale(Vector3.one * 1.2f, 0.5f).SetEase(Ease.OutQuart);
                }
            }
        }
    }

    public bool IsPageActive(PAGES check)
    {
        int index = (int)check;

        if (index < 0 || index > pages.Length) return false;

        return pagesCanvas[index].alpha > 0.5f;
    }

    public void ShowPage(PAGES pageIndex, System.Action callback = null, bool hideTransition = false) {
        if ((int)pageIndex >= pages.Length) return;
        if ((int)pageIndex >= pagesCanvas.Length) return;

        HideAllPages(hideTransition);

        pages[(int)pageIndex].SetActive(true);
        if (hideTransition) {
            //pagesCanvas[pageIndex].blocksRaycasts = true;
            pagesCanvas[(int)pageIndex].alpha = 1.0f;
            if (callback != null)
                callback();
        } else {
            //pagesCanvas[pageIndex].blocksRaycasts = false;
            showTweener = pagesCanvas[(int)pageIndex].DOFade(1.0f, 0.5f).OnComplete(() => {
                pages[(int)pageIndex].SetActive(true);
                if (callback != null)
                    callback();
                //pagesCanvas[pageIndex].blocksRaycasts = true;
            });
            //pages[(int)pageIndex].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuart);

        }
    }
}
