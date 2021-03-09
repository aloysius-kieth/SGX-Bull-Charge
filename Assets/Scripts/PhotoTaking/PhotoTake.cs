#define DEBUG_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;
using DG.Tweening;
using TMPro;
using System.Threading.Tasks;
using Windows.Kinect;

public class PhotoTake : MonoBehaviour
{
    #region SINGLETON
    public static PhotoTake Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        kinectSensor = KinectSensor.GetDefault();
        if (!kinectSensor.IsOpen)
        {
            kinectSensor.Open();
        }
        Debug.Log("is kinect sensor open? " + kinectSensor.IsOpen);
    }
    #endregion
    KinectSensor kinectSensor;

    //bool facebookSent = false;
    //bool emailSent = false;

    [Header("Pages")]
    public CanvasGroup[] pageList;

    [Header("Image References")]
    public RawImage camDisplay;
    public RawImage cachedDisplay;
    public Image photoFrame;
    public Sprite[] frames;

    public Image fbWP_successImg;
    public Image email_successImg;
    public Image skip_successImg;

    public Image fbWP_failImg;
    public Image email_failImg;
    public Image skip_failImg;

    [Header("Buttons")]
    public Button retakeBtn;
    public Button emailBtn;
    public Button skipBtn;

    //public Button postToFBWorkBtn;
    ////public Button emailBtn;
    //public Button submitEmailBtn;
    //public Button skipPostBtn;

    [Header("Inputfields & toggle")]
    public TMP_InputField ifName;
    public TMP_InputField ifEmail;
    public Toggle policyToggle;

    [Header("Text")]
    public TextMeshProUGUI feedbackText;
    //public TextMeshProUGUI feedbackFBText;
    //public TextMeshProUGUI feedbackEmailPhotoText;
    //public TextMeshProUGUI feedbackSkipText;

    string photoPath;
    string photoFileName;

    Texture2D cachedImage;

    public float IdleInterval { get { return TrinaxGlobal.Instance.idleInterval; } }
    float idleTimer;

    [Header("Component References")]
    public KinectManager kinectMan;
    public PrestartCountdownFinish photoCountDown;
    public ParticleSystem[] confettiPS;
    public ParticleSystem fireworkPS;

    void Start()
    {
        SwitchFrame(TrinaxGlobal.Instance.state);
        //StorePrevGameState(TrinaxGlobal.Instance.state);

        Debug.Log("Current gameType " + TrinaxGlobal.Instance.gameType);
        TrinaxGlobal.Instance.state = GAMESTATES.PHOTOTAKING;

        feedbackText.text = "";

        foreach (var ps in confettiPS)
        {
            ps.gameObject.SetActive(false);
        }
        fireworkPS.gameObject.SetActive(false);

        InitButtonListeners();
        policyToggle.isOn = false;

        fbWP_successImg.color = new Color(1, 1, 1, 0);
        email_successImg.color = new Color(1, 1, 1, 0);
        skip_successImg.color = new Color(1, 1, 1, 0);

        fbWP_failImg.color = new Color(1, 1, 1, 0);
        email_failImg.color = new Color(1, 1, 1, 0);
        skip_failImg.color = new Color(1, 1, 1, 0);

        cachedImage = new Texture2D(858, 1288, TextureFormat.RGBA32, false);
        cachedDisplay.texture = cachedImage;

        photoPath = TrinaxGlobal.Instance.photoPath;
        photoFileName = TrinaxGlobal.Instance.photoFileName;

        StartCoroutine(ToPhotoTake());
    }

    //void StorePrevGameState(GAMESTATES state)
    //{
    //    switch (state)
    //    {
    //        case GAMESTATES.DETERMINED:
    //            TrinaxGlobal.Instance.gameType = TrinaxGlobal.BALL_GAMETYPE;
    //            break;
    //        case GAMESTATES.ENERGETIC:
    //            TrinaxGlobal.Instance.gameType = TrinaxGlobal.RUNNING_GAMETYPE;
    //            break;
    //        case GAMESTATES.GENEROUS:
    //            TrinaxGlobal.Instance.gameType = TrinaxGlobal.DONATE_GAMETYPE;
    //            break;
    //        case GAMESTATES.KINDNESS:
    //            TrinaxGlobal.Instance.gameType = TrinaxGlobal.MEMORY_GAMETYPE;
    //            break;
    //    }
    //}

    void SwitchFrame(GAMESTATES state)
    {
        switch (state)
        {
            case GAMESTATES.ENERGETIC:
                photoFrame.sprite = frames[0];
                break;
            case GAMESTATES.DETERMINED:
                photoFrame.sprite = frames[1];
                break;
            case GAMESTATES.KINDNESS:
                photoFrame.sprite = frames[2];
                break;
            //case GAMESTATES.GENEROUS:
            //    photoFrame.sprite = frames[3];
            //    break;
            default:
                break;
        }
    }

    int buttonCount = 0;
    void ButtonCounter()
    {
        buttonCount++;
        if (buttonCount > 1)
        {
            return;
        }
    }

    private void Update()
    {
        if (IsPageActive(PHOTOTAKING_PAGES.PHOTOTAKE))
        {
            camDisplay.texture = kinectMan.GetUsersClrTex();
        }
        else
        {
            camDisplay.texture = null;
        }

        if (IsPageActive(PHOTOTAKING_PAGES.REVIEW) /*|| IsPageActive(PHOTOTAKING_PAGES.POSTPHOTO)*/)
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
    }

    void InitButtonListeners()
    {
        //postToFBWorkBtn.onClick.AddListener(() =>
        //{
        //    TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
        //    OnSubmitFBWP();
        //});

        //submitEmailBtn.onClick.AddListener(() =>
        //{
        //    TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
        //    OnSubmitEmailPhoto().WrapErrors();
        //});

        //emailBtn.onClick.AddListener(() =>
        //{
        //    //ToEmail();
        //});

        retakeBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            RetakePhoto();
        });

        emailBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            
            //ToPostPhoto();
#if DEBUG_MODE
            StartCoroutine(ToCongrats());
#else
            ButtonCounter();
            OnSubmitPhoto(true).WrapErrors();
#endif
        });

        skipBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
#if DEBUG_MODE
        StartCoroutine(ToCongrats());
#else
            ButtonCounter();
            OnSubmitPhoto().WrapErrors();
#endif
        });

        //skipPostBtn.onClick.AddListener(() =>
        //{
        //    TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
        //    OnSkipPostSubmit().WrapErrors();
        //});
    }

    IEnumerator ToPhotoTake()
    {
        SetPageActive(PHOTOTAKING_PAGES.PHOTOTAKE);
        yield return new WaitForSeconds(3.0f);

        StartPhotoTakeCountdown();
    }

    IEnumerator DoRetakePhoto()
    {
        Debug.Log("retaking photo...");
        SetPageActive(PHOTOTAKING_PAGES.PHOTOTAKE);
        yield return new WaitForSeconds(2);

        StartPhotoTakeCountdown();
    }

    void ToReview()
    {
        Debug.Log("to review");
        SetPageActive(PHOTOTAKING_PAGES.REVIEW);
    }

    //Color disableColor = new Color(1, 1, 1, 0.5f);
    //void ToPostPhoto()
    //{
    //    Debug.Log("to postphoto");
    //    SetPageActive(PHOTOTAKING_PAGES.POSTPHOTO);

    //    //check for userType = 3 aka vistor to disable FBWP button
    //    if (TrinaxGlobal.Instance.userType == 3)
    //    {
    //        postToFBWorkBtn.interactable = false;
    //        ColorBlock cb = postToFBWorkBtn.colors;
    //        cb.disabledColor = disableColor;
    //        postToFBWorkBtn.colors = cb;
    //    }
    //}

    //void ToEmail()
    //{
    //    Debug.Log("to email");
    //    SetPageActive(PHOTOTAKING_PAGES.EMAIL);
    //}

    IEnumerator ToCongrats()
    {
        TrinaxAudioManager.Instance.ImmediateStopMusic();
        TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.APPLAUSE);
        TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.CONGRATS, () =>
        {
            TrinaxAudioManager.Instance.PlayMusic(TrinaxAudioManager.AUDIOS.IDLE, true);
        });
        SetPageActive(PHOTOTAKING_PAGES.CONGRATS);
        foreach (var ps in confettiPS)
        {
            ps.gameObject.SetActive(true);
        }
        fireworkPS.gameObject.SetActive(true);

#if !DEBUG_MODE
        //TODO: call server api to dispense gift
        yield return RunCongratsAsync().AsIEnumerator();
#else
        // FOR TESTING PURPOSES, REMOVE LATER
        yield return new WaitForSeconds(10);
        ToThankyou();
#endif
    }

    async Task RunCongratsAsync()
    {
        DispenseSendJsonData sJson = new DispenseSendJsonData();
        sJson.userID = TrinaxGlobal.Instance.pUserId;
        //sJson.dispenseID = TrinaxGlobal.Instance.gameType;

        await TrinaxAsyncServerManager.Instance.Dispense(sJson, async (bool success, DispenseReceiveJsonData rJson) =>
        {
            if (success)
            {
                if (rJson.data == true)
                {
                    Debug.Log("Dispensing " + TrinaxGlobal.Instance.gameType + " gift");
                    await new WaitForSeconds(10f);
                    ToThankyou();
                }
                else
                {
                    Debug.Log("Dispensing <color=red>failed sending to server!</color>!");
                    await new WaitForSeconds(10f);
                    ToThankyou();
                }
            }
            else
            {
                Debug.Log("Dispensing <color=red>communication to server failed!</color>!");
                await new WaitForSeconds(10f);
                ToThankyou();
            }
        });
    }

    async void ToThankyou()
    {
        foreach (var ps in confettiPS)
        {
            ps.gameObject.SetActive(false);
        }
        SetPageActive(PHOTOTAKING_PAGES.THANKYOU);
        await new WaitForSeconds(5f);

        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.SCREENSAVER);
    }

    public void TakePhoto()
    {
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height);
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);

        Camera.main.targetTexture = rt;
        Camera.main.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

        cachedImage.SetPixels(0, 0, 858, 1288, screenShot.GetPixels(100, 250, 858, 1288));
        cachedImage.Apply();
        byte[] jpeg = cachedImage.EncodeToJPG(100);

        File.WriteAllBytes(TrinaxGlobal.Instance.photoPath + string.Format(photoFileName, Application.dataPath), jpeg);
        Debug.Log(string.Format(photoFileName, Application.dataPath));

        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Go to review page
        ToReview();
    }

    void RetakePhoto()
    {
        StartCoroutine(DoRetakePhoto());
    }

    //async Task OnSubmitEmail()
    //{
    //    bool isValidInput = true;

    //    if (string.IsNullOrEmpty(ifName.text))
    //    {
    //        ifName.image.DOColor(Color.red, 0.3f);
    //        ifName.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
    //        {
    //            submitEmailBtn.interactable = true;
    //            ifName.image.DOColor(Color.white, 0.3f);
    //        });

    //        submitEmailBtn.interactable = false;
    //        isValidInput = false;
    //    }

    //    if (!policyToggle.isOn)
    //    {
    //        policyToggle.image.DOColor(Color.red, 0.3f);
    //        policyToggle.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
    //        {
    //            submitEmailBtn.interactable = true;
    //            policyToggle.image.DOColor(Color.white, 0.3f);
    //        });

    //        submitEmailBtn.interactable = false;
    //        isValidInput = false;
    //    }

    //    if (string.IsNullOrEmpty(ifEmail.text))
    //    {
    //        ifEmail.image.DOColor(Color.red, 0.3f);
    //        ifEmail.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
    //        {
    //            submitEmailBtn.interactable = true;
    //            ifEmail.image.DOColor(Color.white, 0.3f);
    //        });

    //        submitEmailBtn.interactable = false;
    //        isValidInput = false;
    //    }
    //    else
    //    {
    //        string str = ifEmail.text.Trim();
    //        int countSymbol = str.Length - str.Replace("@", "").Length;
    //        if (countSymbol != 1 || !str.Contains(".") || str.IndexOf('@') == 0 || str.Contains(" "))
    //        {
    //            ifEmail.image.DOColor(Color.red, 0.3f);
    //            ifEmail.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
    //            {
    //                submitEmailBtn.interactable = true;
    //                ifEmail.image.DOColor(Color.white, 0.3f);
    //            });

    //            submitEmailBtn.interactable = false;
    //            isValidInput = false;
    //        }
    //    }

    //    if (isValidInput)
    //    {
    //        //TODO: play correct sound
    //        TrinaxGlobal.Instance.pEmail = ifEmail.text.Trim();

    //        Debug.Log("Email: " + TrinaxGlobal.Instance.pEmail);

    //        //TODO: call server api to send email/photo to server
    //        await TrinaxAsyncServerManager.Instance.SendPhotoAsync(success =>
    //        {
    //            if (success)
    //            {
    //                Debug.Log("Photo send!");
    //                StartCoroutine(ToCongrats());
    //            }
    //            else
    //            {
    //                Debug.Log("Did not send photo due to some error");
    //            }
    //        });

    //    }
    //    else
    //    {
    //        //TOOD: play wrong sound
    //    }
    //}

    //bool isFBWPSubmitted = false;
    //void OnSubmitFBWP()
    //{
    //    facebookSent = true;
    //    Debug.Log("FBWP: " + facebookSent);
    //    if (emailSent)
    //    {
    //        ToggleInteractivityBtns(false, false, true);
    //    }
    //    else
    //    {
    //        ToggleInteractivityBtns(false, true, true);
    //    }

    //    AddResultSendJsonData sJson = new AddResultSendJsonData();
    //    sJson.result = true;
    //    sJson.score = "";
    //    sJson.chooseEmailSent = false;
    //    sJson.chooseFacebookSent = true;
    //    sJson.amount = TrinaxGlobal.Instance.donateAmount;
    //    sJson.gameType = TrinaxGlobal.Instance.gameType;
    //    sJson.userID = TrinaxAsyncServerManager.Instance.userID;

    //    //TODO: call server api to post to facebook workplace
    //    await TrinaxAsyncServerManager.Instance.AddResult(sJson, (bool success, AddResultReceiveJsonData rJson) =>
    //    {
    //        if (success)
    //        {
    //            if (rJson.data == true)
    //            {
    //                isFBWPSubmitted = true;
    //                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_SUCCESS);
    //                Debug.Log("FBWP success!");
    //                fbWP_successImg.DOFade(1, 1).OnComplete(() =>
    //                {
    //                    fbWP_successImg.DOFade(0, 1);
    //                });
    //                StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackFBText, TrinaxGlobal.SUCCESS_FEEDBACK, 0.75f, 1f));

    //                if (isEmailPhotoSubmitted)
    //                {
    //                    ToggleInteractivityBtns(false, false, true);
    //                }
    //                else
    //                {
    //                    ToggleInteractivityBtns(false, true, true);
    //                }
    //            }
    //            else
    //            {
    //                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
    //                Debug.Log("FBWP failed sending to server!");
    //                fbWP_failImg.DOFade(1, 1).OnComplete(() =>
    //                {
    //                    fbWP_failImg.DOFade(0, 1);
    //                });
    //                StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackFBText, TrinaxGlobal.FAIL_FEEDBACK, 0.75f, 1f));

    //                if (isEmailPhotoSubmitted)
    //                {
    //                    ToggleInteractivityBtns(true, false, true);
    //                }
    //                else
    //                {
    //                    ToggleInteractivityBtns(true, true, true);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
    //            Debug.Log("FBWP communication to server fail!");
    //            fbWP_failImg.DOFade(1, 1).OnComplete(() =>
    //            {
    //                fbWP_failImg.DOFade(0, 1);
    //            });
    //            StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackFBText, TrinaxGlobal.FAIL_FEEDBACK, 0.75f, 1f));

    //            if (isEmailPhotoSubmitted)
    //            {
    //                ToggleInteractivityBtns(true, false, true);
    //            }
    //            else
    //            {
    //                ToggleInteractivityBtns(true, true, true);
    //            }
    //        }
    //    });
    //}

    const int MAX_NUM_OF_TRIES = 3;
    int numOfTries = 0;
    async Task OnSubmitPhoto(bool _chooseEmail = false)
    {
        ToggleInteractivityBtns(false);

        AddResultSendJsonData sJson = new AddResultSendJsonData
        {
            result = true,
            score = "",
            chooseEmailSent = _chooseEmail,
            gameType = TrinaxGlobal.Instance.gameType,
            userID = TrinaxGlobal.Instance.pUserId,
        };

        Debug.Log("Email: " + sJson.chooseEmailSent);

        await TrinaxAsyncServerManager.Instance.AddResult(sJson, (bool success, AddResultReceiveJsonData rJson) =>
        {
            if (success)
            {
                if (rJson.data)
                {
                    TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_SUCCESS);
                    Debug.Log("<AddResult> API: true");
                    // If user has not gotten a gift before...
                    if (!TrinaxGlobal.Instance.Dispensed)
                    {
                        StartCoroutine(ToCongrats());
                    }
                    else
                    {
                        ToThankyou();
                    }
                }
                else
                {
                    TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
                    StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackText, TrinaxGlobal.FAIL_FEEDBACK, 0.5f, 1f, ()=> 
                    {
                        ToggleInteractivityBtns(true);
                        Debug.Log("<AddResult> API: false");

                        if (numOfTries >= MAX_NUM_OF_TRIES)
                        {
                            TrinaxGlobal.Instance.isReturningFromActivity = false;
                            TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.SCREENSAVER);
                        }
                        else numOfTries++;
                    }));
                }
            }
            else
            {
                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
                StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackText, TrinaxGlobal.FAIL_FEEDBACK, 0.5f, 1f, ()=>
                {
                    ToggleInteractivityBtns(true);
                    Debug.Log("<AddResult> API: false");

                    if (numOfTries >= MAX_NUM_OF_TRIES)
                    {
                        TrinaxGlobal.Instance.isReturningFromActivity = false;
                        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.SCREENSAVER);
                    }
                    else numOfTries++;
                }));
            }
        });

        Debug.Log("Current try: " + numOfTries);
    }

    //async Task OnSubmitEmailPhoto()
    //{
    //    emailSent = true;
    //    Debug.Log("EmailSent: " + emailSent);
    //    if (facebookSent)
    //    {
    //        ToggleInteractivityBtns(false, false, true);
    //    }
    //    else
    //    {
    //        ToggleInteractivityBtns(true, false, true);
    //    }

    //    AddResultSendJsonData sJson = new AddResultSendJsonData();
    //    sJson.result = true;
    //    sJson.score = "";
    //    sJson.chooseEmailSent = true;
    //    sJson.chooseFacebookSent = false;
    //    sJson.amount = TrinaxGlobal.Instance.donateAmount;
    //    sJson.gameType = TrinaxGlobal.Instance.gameType;
    //    sJson.userID = TrinaxAsyncServerManager.Instance.userID;

    //    //TODO: call server api to email photo
    //    await TrinaxAsyncServerManager.Instance.AddResult(sJson, (bool success, AddResultReceiveJsonData rJson) =>
    //    {
    //        if (success)
    //        {
    //            if (rJson.data == true)
    //            {
    //                isEmailPhotoSubmitted = true;
    //                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_SUCCESS);
    //                Debug.Log("EmailPhoto success!");
    //                email_successImg.DOFade(1, 1).OnComplete(() =>
    //                {
    //                    email_successImg.DOFade(0, 1);
    //                });
    //                StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackEmailPhotoText, TrinaxGlobal.SUCCESS_FEEDBACK, 0.5f, 1f));

    //                // only staff
    //                if (TrinaxGlobal.Instance.userType == 1)
    //                {
    //                    if (isFBWPSubmitted)
    //                    {
    //                        ToggleInteractivityBtns(false, false, true);
    //                    }
    //                    else
    //                    {
    //                        ToggleInteractivityBtns(true, false, true);
    //                    }
    //                }
    //                // for visitor
    //                else
    //                {
    //                    ToggleInteractivityBtns(false, false, true);
    //                }
    //            }
    //            else
    //            {
    //                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
    //                Debug.Log("EmailPhoto failed sending to server!");
    //                email_failImg.DOFade(1, 1).OnComplete(() =>
    //                {
    //                    email_failImg.DOFade(0, 1);
    //                });
    //                StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackEmailPhotoText, TrinaxGlobal.FAIL_FEEDBACK, 0.5f, 1f));

    //                //for staff
    //                if (TrinaxGlobal.Instance.userType == 1)
    //                {
    //                    if (isFBWPSubmitted)
    //                    {
    //                        ToggleInteractivityBtns(false, true, true);
    //                    }
    //                    else
    //                    {
    //                        ToggleInteractivityBtns(true, true, true);
    //                    }
    //                }
    //                // for visitor
    //                else
    //                {
    //                    ToggleInteractivityBtns(false, true, true);
    //                }

    //            }
    //        }
    //        else
    //        {
    //            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
    //            Debug.Log("EmailPhoto communication to server fail!");
    //            email_failImg.DOFade(1, 1).OnComplete(() =>
    //            {
    //                email_failImg.DOFade(0, 1);
    //            });
    //            StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackEmailPhotoText, TrinaxGlobal.FAIL_FEEDBACK, 0.5f, 1f));

    //            // for staff
    //            if (TrinaxGlobal.Instance.userType == 1)
    //            {
    //                if (isFBWPSubmitted)
    //                {
    //                    ToggleInteractivityBtns(false, true, true);
    //                }
    //                else
    //                {
    //                    ToggleInteractivityBtns(true, true, true);
    //                }
    //            }
    //            //for visitor
    //            else
    //            {
    //                ToggleInteractivityBtns(false, true, true);
    //            }
    //        }
    //    });
    //}

//    async Task OnSkipPostSubmit()
//    {
//        ToggleInteractivityBtns(false, false, false);

//        AddResultSendJsonData sJson = new AddResultSendJsonData();
//        sJson.result = true;
//        sJson.score = "";
//        sJson.chooseEmailSent = emailSent;
//        sJson.gameType = TrinaxGlobal.Instance.gameType;
//        sJson.userID = TrinaxGlobal.Instance.pUserId;

//#if !DEBUG_MODE
//        await TrinaxAsyncServerManager.Instance.AddResult(sJson, (bool success, AddResultReceiveJsonData rJson) =>
//        {
//            if (success)
//            {
//                if (rJson.data == true)
//                {
//                    TrinaxGlobal.Instance.firstRun = false;
//                    TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_SUCCESS);
//                    Debug.Log("Added result success!");
//                    skip_successImg.DOFade(1, 1).OnComplete(() =>
//                    {
//                        skip_successImg.DOFade(0, 1);
//                    });
//                    StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackSkipText, TrinaxGlobal.SUCCESS_FEEDBACK, 0.5f, 1f));

//                    // if userType is staff
//                    if (TrinaxGlobal.Instance.userType == 1)
//                    {
//                        StartCoroutine(ToCongrats());
//                    }
//                    // else all are visitors
//                    else
//                    {
//                        ToThankyou();
//                    }
//                }
//                else
//                {
//                    TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
//                    Debug.Log("Added result failed sending to server!");
//                    skip_failImg.DOFade(1, 1).OnComplete(() =>
//                    {
//                        skip_failImg.DOFade(0, 1);
//                    });
//                    StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackSkipText, TrinaxGlobal.FAIL_FEEDBACK, 0.5f, 1f));
//                    ToggleInteractivityBtns(true, true, true);
//                }
//            }
//            else
//            {
//                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
//                Debug.Log("Added result communication to server fail!");
//                skip_failImg.DOFade(1, 1).OnComplete(() =>
//                {
//                    skip_failImg.DOFade(0, 1);
//                }); ;
//                StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackSkipText, TrinaxGlobal.FAIL_FEEDBACK, 0.5f, 1f));
//                ToggleInteractivityBtns(true, true, true);
//            }
//        });
//#else
//        StartCoroutine(ToCongrats());
//#endif
//    }

    void StartPhotoTakeCountdown()
    {
        Debug.Log("taking photo...");
        photoCountDown.gameObject.SetActive(true);
        photoCountDown.SetCountAndStart();
    }

    void SetPageActive(PHOTOTAKING_PAGES page, System.Action callback = null)
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
                    canvasGrp.DOFade(0f, 0f).OnComplete(() =>
                    {
                        canvasGrp.gameObject.SetActive(false);
                    });
                }
            }
        }
    }

    bool IsPageActive(PHOTOTAKING_PAGES check)
    {
        int index = (int)check;

        if (index < 0 || index > pageList.Length) return false;

        return pageList[index].alpha > 0.5f;
    }

    void ToggleInteractivityBtns(bool enabled)
    {
        retakeBtn.interactable = enabled;
        emailBtn.interactable = enabled;
        skipBtn.interactable = enabled;
    }

}
