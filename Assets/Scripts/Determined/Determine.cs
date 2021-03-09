using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

using DG.Tweening;
using TMPro;
using Windows.Kinect;

using Random = System.Random;

public class Determine : MonoBehaviour
{
    #region SINGLETON
    public static Determine Instance { get; set; }
    private void Awake()
    {
        kinectSensor = KinectSensor.GetDefault();
        if (!kinectSensor.IsOpen)
        {
            kinectSensor.Open();
        }
        Debug.Log("is kinect sensor open? " + kinectSensor.IsOpen);
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        rand = new Random();
    }
    #endregion
    KinectSensor kinectSensor;

    public GameObject[] non_interactiveObjs;

    [Header("Pages")]
    public CanvasGroup[] pageList;

    [Header("Buttons")]
    public Button startBtn;
    public Button backBtn;
    public Button replayBtn;

    [Header("Text")]
    public TextMeshProUGUI passResultText;
    public TextMeshProUGUI failResultText;
    public TextMeshProUGUI numOfSavesText;
    public TextMeshProUGUI totalSavesText;

    [Header("Component Reference")]
    public Hand leftHand;
    public Hand rightHand;
    public CountdownTimer countDown;
    public ParticleSystem confettiPS;
    public ParticleSystem emojiExplodePS;
    public GameObject bull;
    CountdownGameTimer gameTimer;
    public Animator wellDoneAnim;
    //public GameObject fakeBall;

    [HideInInspector]
    public Random rand;

    int ballScore;
    const int maxNumSaves = 10;

    public float IdleInterval { get { return TrinaxGlobal.Instance.idleInterval; } }
    float idleTimer;

    private void Start()
    {
        TrinaxGlobal.Instance.state = GAMESTATES.DETERMINED;
        CountdownTimer.OnCountdownFinished += OnCountdownFinished;

        gameTimer = GetComponent<CountdownGameTimer>();

        InitButtonListeners();
        ShowInstructions();

        bull.SetActive(false);

        emojiExplodePS.Stop();
        wellDoneAnim.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Ball.OnBallHit += OnBallHit;
    }

    private void OnDisable()
    {
        Ball.OnBallHit -= OnBallHit;
    }

    private void Update()
    {
        // Game over
        if (!TrinaxGlobal.Instance.IsGameOver && gameTimer.duration <= 0)
        {
            OnGameEnd();
        }

        //numOfSavesText.text = "0" + ballScore.ToString();
        if (!TrinaxGlobal.Instance.IsGameOver && ballScore >= maxNumSaves)
        {
            StartCoroutine(OnGameEndDelayed(true, 0.1f));
        }

        if (IsPageActive(DETERMINE_PAGES.INSTRUCTIONS) || IsPageActive(DETERMINE_PAGES.FAIL_RESULT))
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

        if (Input.anyKeyDown)
        {
            idleTimer = 0;
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

    bool isTriggeredMsg = false;
    void TriggerMsg()
    {
        if (!TrinaxGlobal.Instance.IsGameOver && IsPageActive(DETERMINE_PAGES.GAME) && !isTriggeredMsg)
        {
            //if (ballScore % 2 == 0 && ballScore != 0)
            //{
            isTriggeredMsg = true;
            StartCoroutine(PlayWellDoneAnim());
            // play audio cue
            RandomAudioCue();
            //}
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
        wellDoneAnim.gameObject.SetActive(true);
        emojiExplodePS.Play();
        yield return new WaitForSeconds(1);
        wellDoneAnim.gameObject.SetActive(false);

        isTriggeredMsg = false;
    }

    public float GetLeftHandVelocity()
    {
        return leftHand.GetVelocity();
    }

    public float GetRightHandVelocity()
    {
        return rightHand.GetVelocity();
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

    void ShowInstructions()
    {
        foreach (var obj in non_interactiveObjs)
        {
            obj.SetActive(false);
        }

        SetGameValues();
        SetPageActive(DETERMINE_PAGES.INSTRUCTIONS);
    }

    void ToCalibration()
    {
        SetPageActive(DETERMINE_PAGES.CALIBRATION);
    }

    void ToPhotoTaking()
    {
        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.PHOTOTAKING);
    }

    public void ToGame()
    {
        leftHand.gameObject.SetActive(true);
        rightHand.gameObject.SetActive(true);
        TrinaxAudioManager.Instance.StopMusic(0.1f);
        foreach (var obj in non_interactiveObjs)
        {
            obj.SetActive(true);
        }
        bull.SetActive(true);
        SetPageActive(DETERMINE_PAGES.GAME, () =>
        {
            countDown.gameObject.SetActive(true);
            countDown.SetCountAndStart();
        });
    }

    void OnBallHit()
    {
        ballScore++;
        TriggerMsg();
        if (ballScore >= 10)
        {
            numOfSavesText.text = ballScore.ToString();
        }
        else
        {
            numOfSavesText.text = "0" + ballScore.ToString();
        }
    }

    void OnCountdownFinished()
    {
        TrinaxGlobal.Instance.IsGameOver = false;
        TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.WHISTLE, () =>
        {
            TrinaxAudioManager.Instance.PlayMusic(TrinaxAudioManager.AUDIOS.GAME, true);
        });
    }

    void OnShowResults(bool _pass)
    {
        if (_pass)
        {
            TrinaxGlobal.Instance.ballActive = false;
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
        SetPageActive(DETERMINE_PAGES.PASS_RESULT);
        passResultText.text = "FANTASTIC" + "\n" + "<size=60%>" + "You made" + " </size>";
        totalSavesText.text = "<line-height=70%>" + "<size=250%>" + ballScore + "</size>" + "\n" + "saves" + "</line-height>";

        yield return new WaitForSeconds(5.0f);

        ToPhotoTaking();
    }

    void ShowFailGame()
    {
        SetPageActive(DETERMINE_PAGES.FAIL_RESULT);
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
            userID = TrinaxGlobal.Instance.pUserId
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
        BallKinectController.Instance.doneCalibrating = false;
        BallKinectController.Instance.calibrationText.text = "Please step back a distance from the screen, until you hear a beep!";
        ShowInstructions();
        gameTimer.Reset(TrinaxGlobal.Instance.ballGameDuration);
    }

    void SetGameValues()
    {
        ballScore = 0;
        totalSavesText.text = "";
        passResultText.text = "";
        failResultText.text = "";
        numOfSavesText.text = "0" + ballScore.ToString();


    }

    void OnGameEnd(bool _complete = false)
    {
        TrinaxAudioManager.Instance.ImmediateStopMusic();
        TrinaxGlobal.Instance.IsGameOver = true;
        ObjectPooler.Instance.ReturnAllToPool();

        wellDoneAnim.gameObject.SetActive(false);

        leftHand.gameObject.SetActive(false);
        rightHand.gameObject.SetActive(false);
        foreach (var obj in non_interactiveObjs)
        {
            obj.SetActive(false);
        }
        bull.SetActive(false);

        // TODO: Add game result

        OnShowResults(_complete);
    }

    IEnumerator OnGameEndDelayed(bool _complete = false, float delay = 0f)
    {
        confettiPS.Play();
        TrinaxGlobal.Instance.IsGameOver = true;

        leftHand.gameObject.SetActive(false);
        rightHand.gameObject.SetActive(false);
        foreach (var obj in non_interactiveObjs)
        {
            obj.SetActive(false);
        }

        yield return new WaitForSeconds(delay);

        OnGameEnd(_complete);
    }

    void SetPageActive(DETERMINE_PAGES page, System.Action callback = null)
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

    public bool IsPageActive(DETERMINE_PAGES check)
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
