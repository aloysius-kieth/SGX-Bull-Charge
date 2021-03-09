using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;
using TMPro;
using System.Threading.Tasks;

public class Energetic : MonoBehaviour
{
    #region SINGLETON
    public static Energetic Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
    #endregion

    [Header("Pages")]
    public CanvasGroup[] pageList;

    [Header("Buttons")]
    public Button startBtn;
    public Button backBtn;
    public Button replayBtn;

    [Header("SprintBar")]
    public Image sprintBar;

    const float MIN_SPRINT_VAL = 1f;

    public float SprintValue { get; set; }
    float sprintDecreaseSpeed = 5f;

    [Header("Text")]
    public TextMeshProUGUI timeTakenText;
    public TextMeshProUGUI passResultText;
    public TextMeshProUGUI failResultText;

    [Header("Component Reference")]
    public CountdownTimer countDown;
    public GameObject[] confettiPS;
    //public ParticleSystem emojiPS;
    //ParticleSystem.EmissionModule emoji_em;
    CountdownGameTimer gameTimer;
    public Animator[] wellDoneAnim;

    float timeTaken = 0f;

    public float IdleInterval { get { return TrinaxGlobal.Instance.idleInterval; } }
    float idleTimer;

    bool gameIsCompleted = false;
    public bool GameIsCompleted { get { return gameIsCompleted; } }

    private void Start()
    {
        TrinaxGlobal.Instance.state = GAMESTATES.ENERGETIC;
        CountdownTimer.OnCountdownFinished += OnCountdownFinished;
        gameTimer = GetComponent<CountdownGameTimer>();

        //emoji_em = emojiPS.emission;
        //emojiPS.SetActive(false);

        InitButtonListeners();

        foreach (var ps in confettiPS)
        {
            ps.SetActive(false);
        }

        foreach (var anim in wellDoneAnim)
        {
            anim.gameObject.SetActive(false);
        }

        ShowInstructions();
    }

    private void Update()
    {
        // Idle checks
        if (IsPageActive(ENERGETIC_PAGES.INSTRUCTIONS) || IsPageActive(ENERGETIC_PAGES.FAIL_RESULT))
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > IdleInterval)
            {
                idleTimer = 0;
                TrinaxGlobal.Instance.isReturningFromActivity = false;
                Debug.Log("Idled for too long!");
                TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.SCREENSAVER);
            }
        }

        // Reset idle
        if (Input.anyKeyDown)
        {
            idleTimer = 0;
        }

        if (TrinaxGlobal.Instance.IsGameOver)
            return;

        // While running
        if (!TrinaxGlobal.Instance.IsGameOver && IsPageActive(ENERGETIC_PAGES.GAME))
        {
            if (RunningKinectController.Instance.IsRunning)
            {
                SprintValue += TrinaxGlobal.Instance.runningPower;
                //SetEmojiPSEmissionRate(7, 7);
            }

            //if (GameIsCompleted && !RunningKinectController.Instance.IsRunning)
            //{
            //    // let player continue to run until kinect detects not running anymore
            //    StopGame();
            //}

            if (!RunningKinectController.Instance.IsRunning)
            {
                //SetEmojiPSEmissionRate(0, 0);
            }
        }

        // Game over condition checks
        if (!TrinaxGlobal.Instance.IsGameOver && IsPageActive(ENERGETIC_PAGES.GAME))
        {
            //sprintValue -= Time.deltaTime / sprintDecreaseSpeed;
            //sprintBar.localScale = new Vector3(SprintValue, 1, 1);
            sprintBar.fillAmount = SprintValue;

            // if bar value goes over 1
            if (SprintValue > 1)
            {
                SprintValue = 1;
                sprintBar.fillAmount = 1;
            }

            // if filled sprint bar, game PASS
            if (SprintValue >= MIN_SPRINT_VAL && gameTimer.duration >= 0)
            {
                //gameIsCompleted = true;
                StopGame();
            }

            // if sprint bar not filled all the way, game FAIL
            else if (SprintValue <= MIN_SPRINT_VAL && gameTimer.duration <= 0)
            {
                //timeTaken = gameTimer.currentTime;
                OnGameEnd();
            }

            // Keyboard Test
            //if (Input.GetKeyDown(KeyCode.Z))
            //{
            //    RunningKinectController.Instance.IsRunning = true;
            //}
            //if (Input.GetKeyDown(KeyCode.X)) 
            //{
            //    RunningKinectController.Instance.IsRunning = false;
            //}
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

        TriggerMsg();
    }

    bool isTriggeredMsg = false;
    void TriggerMsg()
    {
        if (!TrinaxGlobal.Instance.IsGameOver && IsPageActive(ENERGETIC_PAGES.GAME) && !isTriggeredMsg)
        {
            float truncated = (float)(System.Math.Truncate((double)SprintValue * 100.0) / 100.0);

            if (truncated % 0.25f == 0 && truncated != 0)
            {
                isTriggeredMsg = true;
                // Trigger msg
                StartCoroutine(PlayWellDoneAnim());
                RandomAudioCue();
            }
        }
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

    IEnumerator PlayWellDoneAnim()
    {
        foreach (var anim in wellDoneAnim)
        {
            anim.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(1);
        foreach (var anim in wellDoneAnim)
        {
            anim.gameObject.SetActive(false);
        }
        isTriggeredMsg = false;
    }

    void InitButtonListeners()
    {
        startBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            ToCalibration();
        });

        backBtn.onClick.AddListener(() =>
        {
            TrinaxGlobal.Instance.isReturningFromActivity = true;
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.SCREENSAVER);
        });

        replayBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            ReplayGame();
        });
    }

    //void SetEmojiPSEmissionRate(float min, float max)
    //{
    //    emoji_em.rateOverTime = new ParticleSystem.MinMaxCurve(min, max);
    //}

    void ShowInstructions()
    {
        SetGameValues();
        SetPageActive(ENERGETIC_PAGES.INSTRUCTIONS);
    }

    void ToCalibration()
    {
        SetPageActive(ENERGETIC_PAGES.CALIBRATION);
    }

    public void ToGame()
    {
        TrinaxAudioManager.Instance.StopMusic(0.1f);
        SetPageActive(ENERGETIC_PAGES.GAME, () =>
        {
            countDown.gameObject.SetActive(true);
            countDown.SetCountAndStart();
        });
    }

    void ToPhotoTaking()
    {
        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.PHOTOTAKING);
    }

    void OnCountdownFinished()
    {
        TrinaxGlobal.Instance.IsGameOver = false;
        TrinaxAudioManager.Instance.PlayMusic(TrinaxAudioManager.AUDIOS.GAME, true);
    }

    void OnShowResults(bool _pass)
    {
        if (_pass)
        {
            TrinaxGlobal.Instance.runningActive = false;
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.APPLAUSE);
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
        SetPageActive(ENERGETIC_PAGES.PASS_RESULT);
        //timeTakenText.text = "<size=180%>" + timeTaken.ToString("F2") + "</size>" + "sec";

        yield return new WaitForSeconds(5.0f);

        ToPhotoTaking();
    }

    void ShowFailGame()
    {
        SetPageActive(ENERGETIC_PAGES.FAIL_RESULT);
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
        RunningKinectController.Instance.doneCalibrating = false;
        RunningKinectController.Instance.calibrationText.text = "Please step back a distance from the screen, until you hear a beep!";
        ShowInstructions();
    }

    void SetGameValues()
    {
        //emojiPS.SetActive(false);
        //SetEmojiPSEmissionRate(0.0f, 0.0f);
        timeTakenText.text = "";
        gameTimer.Reset(TrinaxGlobal.Instance.runningGameDuration);
        SprintValue = 0f;
        sprintBar.fillAmount = 0;
    }

    void OnGameEnd(bool _complete = false)
    {
        TrinaxGlobal.Instance.IsGameOver = true;

        TrinaxAudioManager.Instance.ImmediateStopMusic();
        OnShowResults(_complete);
    }

    void StopGame()
    {
        timeTaken = gameTimer.duration;
        StartCoroutine(OnGameEndDelayed(true, 0.1f));
    }

    IEnumerator OnGameEndDelayed(bool _complete = false, float delay = 0f)
    {
        foreach (var ps in confettiPS)
        {
            ps.SetActive(true);
        }

        TrinaxGlobal.Instance.IsGameOver = true;

        yield return new WaitForSeconds(delay);

        OnGameEnd(_complete);
    }

    void SetPageActive(ENERGETIC_PAGES page, System.Action callback = null)
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

    public bool IsPageActive(ENERGETIC_PAGES check)
    {
        int index = (int)check;

        if (index < 0 || index > pageList.Length) return false;

        return pageList[index].alpha > 0.5f;
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
