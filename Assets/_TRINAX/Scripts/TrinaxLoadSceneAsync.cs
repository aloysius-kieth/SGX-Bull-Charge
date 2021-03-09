using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class TrinaxLoadSceneAsync : MonoBehaviour
{
    #region SINGLETON
    public static TrinaxLoadSceneAsync Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    private AsyncOperation async;
    public GameObject loadingScreen;
    public Image imageBG;
    public Image imageFillBG;
    public Image imageFillOverlay;
    public Image bull;
    public TextMeshProUGUI[] loadingNum;

    public bool useNum = true;

    void Start()
    {
        imageFillBG.DOFade(0, 0);
        imageFillOverlay.DOFade(0, 0);
        bull.DOFade(0, 0);

        foreach (TextMeshProUGUI child in loadingNum)
        {
            child.DOFade(0, 0);
        }

        loadingScreen.SetActive(false);
    }

    public void LoadLevel(int sceneIndex, TweenCallback callback)
    {
        loadingScreen.SetActive(true);

        loadingNum[0].text = "0";
        imageFillOverlay.fillAmount = 0.0f;
        gameObject.SetActive(true);
        if (useNum)
        {
            foreach (TextMeshProUGUI child in loadingNum)
            {
                child.DOFade(1, 1);
            }
        }
        imageFillOverlay.DOFade(1, 1);
        imageBG.DOFade(0.75f, 1);
        bull.DOFade(1, 1);
        imageFillBG.DOFade(1, 1).OnComplete(() => {
            StartCoroutine(LoadLevelAsync(sceneIndex, callback));
        });

    }

    IEnumerator LoadLevelAsync(int sceneIndex, TweenCallback callback)
    {
        async = SceneManager.LoadSceneAsync(sceneIndex);
        Debug.Log(async);
        float progress = 0;

        while (!async.isDone)
        {
            progress = Mathf.Clamp01(async.progress / 0.9f);
            imageFillOverlay.fillAmount = progress;
            if (useNum)
            {
                loadingNum[0].text = Mathf.RoundToInt(progress * 100).ToString();
            }
            yield return null;
        }
        if (async.isDone)
        {
            imageFillOverlay.fillAmount = progress;
            if (useNum)
            {
                loadingNum[0].text = Mathf.RoundToInt(progress * 100).ToString();
            }
            imageFillBG.DOFade(0, 0.5f);
            imageBG.DOFade(0, 0.5f);
            bull.DOFade(0, 0.5f);
            imageFillOverlay.DOFade(0, 0.5f).OnComplete(callback);
            if (useNum)
            {
                foreach (TextMeshProUGUI child in loadingNum)
                {
                    child.DOFade(0, 1);

                }

                loadingScreen.SetActive(false);
                //async = null;
            }
            //loadingNum[0].text = "0";
        }
    }
}
