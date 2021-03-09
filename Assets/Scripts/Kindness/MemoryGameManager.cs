using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;

public class MemoryGameManager : MonoBehaviour
{
    #region SINGLETON
    public static MemoryGameManager Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
    #endregion

    public int rows = 4;
    public int cols = 3;
    public float offsetX = 4f;
    public float offsetY = 5f;

    public Card originalCard;
    public Transform startCardPos;
    [SerializeField]
    Sprite[] images;

    Vector3 startPos;
    int[] numbers = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };

    int totalCards;

    [Header("Pages")]
    public CanvasGroup[] pageList;

    [Header("Buttons", order = 0)]
    public Button startGameBtn;
    public Button backBtn;
    public Button replayBtn;

    [Header("Text")]
    public TextMeshProUGUI passResultText;
    public TextMeshProUGUI failResultText;
    public TextMeshProUGUI memoriseText;

    [Header("Component Reference")]
    public CountdownTimer countDown;
    CountdownGameTimer gameTimer;
    public ParticleSystem confettiPS;

    public const float durationToShowCards = 5;
    float duration;

    public float IdleInterval { get { return TrinaxGlobal.Instance.idleInterval; } }
    float idleTimer;

    private void Start()
    {
        confettiPS.Stop();

        TrinaxGlobal.Instance.state = GAMESTATES.KINDNESS;
        CountdownTimer.OnCountdownFinished += OnCountdownFinished;
        gameTimer = GetComponent<CountdownGameTimer>();

        InitButtonListeners();
        ShowInstructions();

        totalCards = rows * cols;

        startPos = startCardPos.transform.position;
        numbers = ShuffleArray(numbers);
    }

    private void Update()
    {
        //if (TrinaxGlobal.Instance.IsGameOver)
        //    return;

        // Pass activity if all cards are matched
        if (!TrinaxGlobal.Instance.IsGameOver && score >= totalCards / 2 )
        {
            StartCoroutine(OnGameEndDelayed(true, 0.1f));
        }
        if(!TrinaxGlobal.Instance.IsGameOver && gameTimer.duration <= 0)
        {
            OnGameEnd();
        }

        if (IsPageActive(KINDNESS_PAGES.INSTRUCTIONS) || IsPageActive(KINDNESS_PAGES.FAIL_RESULT))
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > IdleInterval)
            {
                Debug.Log("Idled for too long!");
                TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.SCREENSAVER);
            }
        }

        if (Input.anyKeyDown)
        {
            idleTimer = 0;
        }

        memoriseText.text = "<size=250%>" + "<space=5em>" + duration.ToString() + " sec" + "</size>" + "\n" + "We have 6 beneficiaries, memorise them!";
        if (duration <= 0)
        {
            memoriseText.alpha = 0;
            StopCoroutine(MemoriseCountdown());
        }

        if (TrinaxGlobal.Instance.IsDevMode)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                AutoWin();
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                AutoLose();
            }
        }
    }

    #region UI IMPLEMENTATIONS
    void InitButtonListeners()
    {
        startGameBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            SetPageActive(KINDNESS_PAGES.GAME, () =>
            {
                StartCoroutine(DoCountdown());
                SpawnCards();
            });
        });

        backBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.SCREENSAVER);
        });

        replayBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            ReplayGame();
        });
    }

    IEnumerator DoCountdown()
    {
        StartCoroutine(MemoriseCountdown());

        yield return new WaitUntil(() => duration == 0);

        TrinaxAudioManager.Instance.StopMusic(0.1f);
        gameTimer.timerText.DOFade(1.0f, 0.25f);
        gameTimer.secText.DOFade(1.0f, 0.25f);
        countDown.gameObject.SetActive(true);
        countDown.SetCountAndStart();
    }

    void ShowInstructions()
    {
        SetGameValues();
        SetPageActive(KINDNESS_PAGES.INSTRUCTIONS);
    }

    IEnumerator MemoriseCountdown()
    {
        while (duration > 0)
        {
            yield return new WaitForSeconds(1);
            if (duration != 1)
            {
                TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.TICK);
            }

            duration--;
        }
    }

    void ToPhotoTaking()
    {
        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.PHOTOTAKING);
    }

    void OnCountdownFinished()
    {
        TrinaxGlobal.Instance.IsGameOver = false;
        TrinaxAudioManager.Instance.PlayMusic(TrinaxAudioManager.AUDIOS.GAME1, true);
    }

    void OnShowResults(bool _pass)
    {
        if (_pass)
        {
            TrinaxGlobal.Instance.memoryActive = false;
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.APPLAUSE);
            //TrinaxAudioManager.Instance.PlayMusic(TrinaxAudioManager.AUDIOS.GAME_WIN, true, 3, false);
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.GAME_WIN, () =>
            {
                TrinaxAudioManager.Instance.PlayMusic(TrinaxAudioManager.AUDIOS.IDLE, true, 3, true);
            });
            StartCoroutine(ShowPassGame());
        }
        else
        {
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.GAME_LOSE, () =>
            {
                TrinaxAudioManager.Instance.PlayMusic(TrinaxAudioManager.AUDIOS.IDLE, true, 3, true);
            });
            ShowFailGame();
        }
    }

    IEnumerator ShowPassGame()
    {
        SetPageActive(KINDNESS_PAGES.PASS_RESULT);
        passResultText.text = "WONDERFUL!" + "\n" + "<size=40%>" + "You found all the matching pairs!" + " </size>";

        yield return new WaitForSeconds(5.0f);

        ToPhotoTaking();
    }

    void ShowFailGame()
    {
        SetPageActive(KINDNESS_PAGES.FAIL_RESULT);
        failResultText.text = "Oh no, don't give up!";

        RunAddLoseResult().WrapErrors();
    }

    async Task RunAddLoseResult()
    {
        AddResultSendJsonData sJson = new AddResultSendJsonData
        {
            result = false,
            score = "",
            chooseEmailSent = false,
            gameType = TrinaxGlobal.Instance.gameType,
            userID = TrinaxGlobal.Instance.pUserId,
        };

        await TrinaxAsyncServerManager.Instance.AddResult(sJson, (bool success, AddResultReceiveJsonData rJson) =>
        {
            if (success)
            {
                if (rJson.data) Debug.Log("<AddResult> API: true");
                else Debug.Log("<AddResult> API: false");
            }
            else
            {
                Debug.Log("<AddResult> API: false");
            }
        });
    }


    void ReplayGame()
    {
        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.KINDNESS);
        //ShowInstructions();
        //gameTimer.Reset(TrinaxGlobal.Instance.memoryGameDuration);
    }

    void SetGameValues()
    {
        gameTimer.timerText.alpha = 0;
        gameTimer.secText.alpha = 0;
        duration = durationToShowCards;
        score = 0;
        memoriseText.text = "";
    }

    void OnGameEnd(bool _complete = false)
    {
        TrinaxGlobal.Instance.IsGameOver = true;

        TrinaxAudioManager.Instance.ImmediateStopMusic();
        OnShowResults(_complete);
    }

    IEnumerator OnGameEndDelayed(bool _complete = false, float delay = 0f)
    {
        confettiPS.Play();
        TrinaxGlobal.Instance.IsGameOver = true;

        yield return new WaitForSeconds(delay);

        OnGameEnd(_complete);
    }

    void SetPageActive(KINDNESS_PAGES page, System.Action callback = null)
    {
        int pageNum = (int)page;
        for (int i = 0; i < pageList.Length; i++)
        {
            CanvasGroup canvasGrp = pageList[i];
            // If on current page...
            if (i == pageNum)
            {
                canvasGrp.gameObject.SetActive(true);
                canvasGrp.blocksRaycasts = true;
                canvasGrp.DOFade(1.0f, 0.5f).OnComplete(() =>
                {
                    callback?.Invoke();
                });
            }
            else
            {
                canvasGrp.blocksRaycasts = false;
                if (canvasGrp.alpha > 0.1f)
                {
                    canvasGrp.DOFade(0f, 0.0001f).OnComplete(() =>
                    {
                        canvasGrp.gameObject.SetActive(false);
                    });
                }
            }
        }
    }

    bool IsPageActive(KINDNESS_PAGES check)
    {
        int index = (int)check;

        if (index < 0 || index > pageList.Length) return false;

        return pageList[index].alpha > 0.5f;
    }
    #endregion

    void SpawnCards()
    {
        GameObject parentCard = new GameObject("CardList");
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Card card;
                //if (i == 0 && j == 0)
                //{
                //    card = originalCard;
                //}
                //else
                //{
                    card = Instantiate(originalCard) as Card;
                    card.transform.SetParent(parentCard.transform);

                    //card.cardBack.GetComponent<SpriteRenderer>().DOColor(new Color(1, 1, 1, 1), 2.5f).OnComplete(() =>
                    //{
                    //    card.GetComponent<SpriteRenderer>().DOColor(new Color(1, 1, 1, 1), 0.1f);
                    //}).SetEase(Ease.Flash);
                //}

                int index = j * cols + i;
                int id = numbers[index];
                card.ChangeSprite(id, images[id]);

                float posX = (offsetX * i) + startPos.x;
                float posY = (offsetY * j) + startPos.y;
                card.transform.position = new Vector3(posX, posY - 10f, startPos.z);
                card.transform.DOMove(new Vector3(posX, posY, startPos.z), 0.5f);


                //card.transform.DORotate(new Vector3(0, 0, 0), 0.25f);
                //yield return new WaitForSeconds(3.0f);

                //card.transform.DORotate(new Vector3(0,180, 0), 0.25f);

            }
        }
    }

    int[] ShuffleArray(int[] numbers)
    {
        // Make a exact copy of the array passed in
        int[] newArray = numbers.Clone() as int[];

        for (int i = 0; i < newArray.Length; i++)
        {
            // Get the first value out of the array
            int temp = newArray[i];
            // Get a random value from 1 to the length of the array
            int r = Random.Range(1, newArray.Length);
            // Assign the randomed value back to the array
            newArray[i] = newArray[r];
            // Get the next value out of the array
            newArray[r] = temp;
        }
        return newArray;
    }

    Card _firstCard;
    Card _secondCard;

    int score;
    float timeBetweenFlip = 0.5f;

    public bool canReveal
    {
        get { return _secondCard == null; }
    }

    public void CardRevealed(Card card)
    {
        if (_firstCard == null)
        {
            _firstCard = card;
        }
        else
        {
            _secondCard = card;
            StartCoroutine("CheckMatch");
        }
    }

    IEnumerator CheckMatch()
    {
        if (_firstCard.ID == _secondCard.ID)
        {
            score++;
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.FEEDBACK_SUCCESS);
            RandomAudioCue();
        }
        else
        {
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
            yield return new WaitForSeconds(timeBetweenFlip);
            _firstCard.OnReveal();
            _secondCard.OnReveal();
        }

        _firstCard = null;
        _secondCard = null;
    }

    bool isWowPlayed = false;
    bool isTerrficPlayed = false;
    void RandomAudioCue()
    {
        if (!isWowPlayed && !isTerrficPlayed)
        {
            TrinaxAudioManager.Instance.PlaySFX2(TrinaxAudioManager.AUDIOS.WOW_CUE);
            isWowPlayed = true;
            isTerrficPlayed = false;
        }
        else if (isWowPlayed && !isTerrficPlayed)
        {
            TrinaxAudioManager.Instance.PlaySFX2(TrinaxAudioManager.AUDIOS.TERRIFIC_CUE);
            isWowPlayed = false;
            isTerrficPlayed = true;
        }
        else if (isTerrficPlayed && !isWowPlayed)
        {
            TrinaxAudioManager.Instance.PlaySFX2(TrinaxAudioManager.AUDIOS.WOW_CUE);
            isWowPlayed = true;
            isTerrficPlayed = false;
        }
    }

    [ContextMenu("AUTOWIN")]
    void AutoWin()
    {
        StartCoroutine(OnGameEndDelayed(true, 0.1f));
    }

    [ContextMenu("AUTOLOSE")]
    void AutoLose()
    {
        OnGameEnd();
    }

}
