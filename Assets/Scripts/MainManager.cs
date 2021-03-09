#define DEBUG_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

using System.Threading.Tasks;

enum MM_Btns
{
    DETERMINE,
    ENERGETIC,
    KINDNESS,
    GENEROUS,
}

public class MainManager : MonoBehaviour
{
    public float IdleInterval { get { return TrinaxGlobal.Instance.idleInterval; } }
    float idleTimer = 0;

    [Header("Pages")]
    public CanvasGroup[] pageList;

    [Header("Buttons")]
    public Button energeticBtn;
    public Button determinationBtn;
    public Button generousBtn;
    public Button kindnessBtn;

    public Button startBtn;

    public Button submitEmailBtn;
    public Toggle policyToggle;
    public Toggle policy2Toggle;

    public Button OTPEnterBtn;
    public Button OTPResendBtn;

    [Header("Text")]
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI stockCheckText;
    public TextMeshProUGUI OTPFeedbackText;
    public TextMeshProUGUI disclaimerText;

    [Header("Inputfields")]
    public TMP_InputField ifEmail;
    public TMP_InputField ifName;
    public TMP_InputField ifMobile;
    public TMP_InputField OTPIF;

    [Header("Animators")]
    public Animator anim;
    public Canvas[] animCanvas;

    #region SINGLETON
    public static MainManager Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }
    #endregion

    private void Start()
    {
        //Debug.Log("isFirstRun " + TrinaxGlobal.Instance.firstRun);
        InitButtonListeners();

        ToScreensaver();
    }

    private void Update()
    {
        if (!IsPageActive(PAGES.SCREENSAVER))
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
        determinationBtn.onClick.AddListener(() =>
        {
            TrinaxGlobal.Instance.gameType = TrinaxGlobal.BALL_GAMETYPE;
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.EMOJI_CLICK);
            StartCoroutine(DoDetermine(() =>
            {
                TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.DETERMINED);
            }));

#if DEBUG_MODE
            StartCoroutine(DoDetermine(() =>
            {
                TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.DETERMINED);
            }));
#else
            //// if userType is visitor
            //if (TrinaxGlobal.Instance.userType == 3)
            //{
            //    TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.EMOJI_CLICK);
            //    StartCoroutine(DoDetermine(() =>
            //    {
            //        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.DETERMINED);
            //    }));
            //}
            //// userType is staff
            //else
            //{
            //    RunStockCheck(() =>
            //    {
            //        StartCoroutine(DoDetermine(() =>
            //        {
            //            TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.DETERMINED);
            //        }));
            //    }).WrapErrors();
            //}
#endif
        });

        energeticBtn.onClick.AddListener(() =>
        {
            TrinaxGlobal.Instance.gameType = TrinaxGlobal.RUNNING_GAMETYPE;
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.EMOJI_CLICK);
            StartCoroutine(DoEnergetic(() =>
            {
                TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.ENERGETIC);
            }));

#if DEBUG_MODE
            StartCoroutine(DoEnergetic(() =>
            {
                TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.ENERGETIC);
            }));
#else
            //// if userType is visitor
            //if (TrinaxGlobal.Instance.userType == 3)
            //{
            //    TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.EMOJI_CLICK);
            //    StartCoroutine(DoEnergetic(() =>
            //    {
            //        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.ENERGETIC);
            //    }));
            //}
            //// userType is staff
            //else
            //{
            //    RunStockCheck(() =>
            //    {
            //        StartCoroutine(DoEnergetic(() =>
            //        {
            //            TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.ENERGETIC);
            //        }));
            //    }).WrapErrors();
            //}
#endif
        });

        kindnessBtn.onClick.AddListener(() =>
        {
            TrinaxGlobal.Instance.gameType = TrinaxGlobal.MEMORY_GAMETYPE;
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.EMOJI_CLICK);
            StartCoroutine(DoKindness(() =>
            {
                TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.KINDNESS);
            }));

#if DEBUG_MODE
            StartCoroutine(DoKindness(() =>
            {
                TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.KINDNESS);
            }));
#else
            //if (TrinaxGlobal.Instance.userType == 3)
            //{
            //    TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.EMOJI_CLICK);
            //    StartCoroutine(DoKindness(() =>
            //    {
            //        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.KINDNESS);
            //    }));
            //}
            //else
            //{
            //    RunStockCheck(() =>
            //    {
            //        StartCoroutine(DoKindness(() =>
            //        {
            //            TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.KINDNESS);
            //        }));
            //    }).WrapErrors();
            //}
#endif
        });

        generousBtn.onClick.AddListener(() =>
        {
            TrinaxGlobal.Instance.gameType = TrinaxGlobal.DONATE_GAMETYPE;
            TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.EMOJI_CLICK);

#if DEBUG_MODE
            //StartCoroutine(DoGenerous(() =>
            //{
            //    TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.GENEROUS);
            //    //TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.PHOTOTAKING);
            //}));
#else
            //if (TrinaxGlobal.Instance.userType == 3)
            //{
            //    TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.EMOJI_CLICK);
            //    StartCoroutine(DoGenerous(() =>
            //    {
            //        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.GENEROUS);
            //    }));
            //}
            //else
            //{
            //    RunStockCheck(() =>
            //    {
            //        StartCoroutine(DoGenerous(() =>
            //        {
            //            TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.GENEROUS);
            //        }));
            //    }).WrapErrors();
            //}
#endif
        });

        submitEmailBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            OnSubmitDetails();
        });

        startBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.START_APP);
            ToLogin();
        });

        OTPEnterBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            OnSubmitOTP();
        });

        OTPResendBtn.onClick.AddListener(() =>
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
            // TODO: Call API to resend OTP
            DoRegister().WrapErrors();
        });

        policyToggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(policyToggle); });
        policy2Toggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(policyToggle); });

        policyToggle.isOn = false;
        policy2Toggle.isOn = false;
    }

    void OnToggleValueChanged(Toggle toggle)
    {
        TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.TYPING);
    }

    IEnumerator DoDetermine(System.Action callback)
    {
        anim.SetTrigger("ToDetermine");
        animCanvas[(int)MM_Btns.DETERMINE].sortingOrder = -1;
        animCanvas[(int)MM_Btns.KINDNESS].sortingOrder = -1;
        animCanvas[(int)MM_Btns.GENEROUS].sortingOrder = -1;

        yield return new WaitForSeconds(2.0f);
        callback?.Invoke();
    }

    IEnumerator DoEnergetic(System.Action callback)
    {
        anim.SetTrigger("ToEnergetic");
        animCanvas[(int)MM_Btns.ENERGETIC].sortingOrder = -1;
        animCanvas[(int)MM_Btns.KINDNESS].sortingOrder = -1;
        animCanvas[(int)MM_Btns.GENEROUS].sortingOrder = -1;
        yield return new WaitForSeconds(2.0f);
        callback?.Invoke();
    }

    IEnumerator DoKindness(System.Action callback)
    {
        anim.SetTrigger("ToKindness");
        animCanvas[(int)MM_Btns.DETERMINE].sortingOrder = -1;
        animCanvas[(int)MM_Btns.ENERGETIC].sortingOrder = -1;
        animCanvas[(int)MM_Btns.GENEROUS].sortingOrder = -1;

        yield return new WaitForSeconds(2.0f);
        callback?.Invoke();
    }

    IEnumerator DoGenerous(System.Action callback)
    {
        anim.SetTrigger("ToGenerous");
        animCanvas[(int)MM_Btns.DETERMINE].sortingOrder = -1;
        animCanvas[(int)MM_Btns.KINDNESS].sortingOrder = -1;
        animCanvas[(int)MM_Btns.ENERGETIC].sortingOrder = -1;

        yield return new WaitForSeconds(2.0f);
        callback?.Invoke();
    }

    void ToScreensaver()
    {
#if DEBUG_MODE
        startBtn.interactable = true;
#else
        startBtn.interactable = false;
#endif
        stockCheckText.alpha = 0;
#if !DEBUG_MODE
        RunStockCheck().WrapErrors();
#endif

        anim.enabled = false;

        TrinaxGlobal.Instance.ResetAllGlobalValues();

        ifName.text = "";
        ifEmail.text = "";
        ifMobile.text = "";

        TrinaxGlobal.Instance.pUserId = "";
        //TrinaxGlobal.Instance.pName = "";
        TrinaxGlobal.Instance.pEmail = "";
        //TrinaxGlobal.Instance.pMobile = "";
        //TrinaxGlobal.Instance.pOTP = "";

        feedbackText.text = "";
        OTPFeedbackText.text = "";
        TrinaxGlobal.Instance.state = GAMESTATES.SCREENSAVER;
        SetPageActive(PAGES.SCREENSAVER);

        //if (TrinaxGlobal.Instance.firstRun)
        //{
        //    RunEnableNoteAcceptor().WrapErrors();
        //}
        //else
        //{
        //    RunDisableNoteAcceptor().WrapErrors();
        //}

        if (TrinaxGlobal.Instance.isReturningFromActivity)
        {
            return;
        }else
        {
            TrinaxAudioManager.Instance.RestoreIdleBGM();
        }
    }

    void ToLogin()
    {
        SetPageActive(PAGES.LOGIN);
        submitEmailBtn.interactable = true;
    }

    async Task RunStockCheck()
    {
        await TrinaxAsyncServerManager.Instance.CheckStock((bool success, StockCheckReceiveJsonData rJson) =>
        {
            if (success)
            {
                Debug.Log("Checking for stock success!");
                // Activity still has stock!
                if (rJson.total > 0)
                {
                    Debug.Log("Machine has stock of: " + rJson.total);
                    startBtn.interactable = true;
                }
                // Activity no stock!
                else
                {
                    Debug.Log("Machine has stock of: " + rJson.total);
                    startBtn.interactable = false;
                    TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
                    stockCheckText.DOFade(1.0f, 0.5f);
                   // StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(stockCheckText, TrinaxGlobal.OUT_OF_STOCK_FEEDBACK, 0, 2.0f));
                }
            }
            else
            {
                Debug.Log("Check stock communication to server failed!");
                startBtn.interactable = false;
                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
                stockCheckText.DOFade(1.0f, 0.5f);
                startBtn.interactable = false;
                //StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(stockCheckText, TrinaxGlobal.FAIL_FEEDBACK, 0, 2.0f));
            }
        });
    }

    async Task DoRegister()
    {
        if (IsPageActive(PAGES.OTP)) OTPResendBtn.interactable = false;

        RegisterSendJsonData sJson = new RegisterSendJsonData();
        //sJson.name = TrinaxGlobal.Instance.pName;
        sJson.email = TrinaxGlobal.Instance.pEmail;
        //sJson.phone = TrinaxGlobal.Instance.pMobile;

        await TrinaxAsyncServerManager.Instance.Register(sJson, (bool success, RegisterReceiveJsonData rJson) =>
        {
            if (success)
            {
                if (rJson.data != null)
                {
                    Debug.Log("Register success!");
                    TrinaxGlobal.Instance.pUserId = rJson.data.userID;
                    TrinaxGlobal.Instance.Dispensed = rJson.data.dispense;
                    Debug.Log(rJson.data.dispense);
                    //ToOTP();
                    ToMainMenu();
                }
                else
                {
                    Debug.Log("Register failed sending to server!");
                    StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackText, TrinaxGlobal.FAIL_FEEDBACK, 1.0f, 2.0f));
                    submitEmailBtn.interactable = true;

                    if (IsPageActive(PAGES.OTP))    OTPResendBtn.interactable = true;
                }
            }
            else
            {
                Debug.Log("Register communication to server failed!");
                StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(feedbackText, TrinaxGlobal.FAIL_FEEDBACK, 1.0f, 2.0f));
                submitEmailBtn.interactable = true;

                if (IsPageActive(PAGES.OTP)) OTPResendBtn.interactable = true;
            }
        });
    }

    //async Task RunCheckOTP()
    //{
    //    await TrinaxAsyncServerManager.Instance.CheckOTP((bool success, OTPRecieveJsonData rJson) =>
    //    {
    //        if (success)
    //        {
    //            if (rJson.data)
    //            {
    //                Debug.Log("OTP success!");
    //                ToMainMenu();
    //            }
    //            else
    //            {
    //                Debug.Log("OTP failed!");
    //                TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
    //                StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(OTPFeedbackText, "Invalid OTP code entered!", 1.0f, 2.0f));
    //                OTPEnterBtn.interactable = true;
    //                OTPResendBtn.interactable = true;
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log("Failed on server level");
    //            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
    //            StartCoroutine(TrinaxGlobal.Instance.ShowFeedbackMsg(OTPFeedbackText, TrinaxGlobal.FAIL_FEEDBACK, 1.0f, 2.0f));
    //            OTPEnterBtn.interactable = true;
    //            OTPResendBtn.interactable = true;
    //        }
    //    });
    //}

    //async Task RunEnableNoteAcceptor()
    //{
    //    await TrinaxAsyncServerManager.Instance.EnableDonate((bool success, EnableReceiveJsonData rJson) =>
    //    {
    //        if (success)
    //        {
    //            if (rJson.data.result == true)
    //            {
    //                Debug.Log("Note Acceptor <color=green>ACTIVATED!</color>");
    //                RunDisableNoteAcceptor().WrapErrors();
    //            }
    //            else
    //            {
    //                Debug.Log("Note Acceptor <color=red>NOT ACTIVATED!</color>");
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log("Note Acceptor <color=red>NOT ACTIVATED!</color>");
    //            // communication to note acceptor failed, thus call it again to activate
    //        }
    //    });
    //}

    //async Task RunDisableNoteAcceptor()
    //{
    //    await TrinaxAsyncServerManager.Instance.DisableDonate((bool success, DisableReceiveJsonData rJson) =>
    //    {
    //        if (success)
    //        {
    //            if (rJson.data.result == true)
    //            {
    //                Debug.Log("Note Acceptor <color=green>DISABLED!</color>");
    //            }
    //            else
    //            {
    //                Debug.Log("Note Acceptor <color=red>STILL RUNNING!</color>");
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log("Note Acceptor <color=red>STILL RUNNING!</color>");
    //        }
    //    });
    //}

    Color disableColor = new Color(1, 1, 1, 0.2f);
    void ToMainMenu()
    {
        anim.enabled = true;
        SetPageActive(PAGES.MAINMENU);

        if (!TrinaxGlobal.Instance.runningActive)
        {
            energeticBtn.interactable = false;
            ColorBlock cb = energeticBtn.colors;
            cb.disabledColor = disableColor;
            energeticBtn.colors = cb;
        }
        if(!TrinaxGlobal.Instance.ballActive)
        {
            determinationBtn.interactable = false;
            ColorBlock cb = energeticBtn.colors;
            cb.disabledColor = disableColor;
            determinationBtn.colors = cb;
        }
        if (!TrinaxGlobal.Instance.memoryActive)
        {
            kindnessBtn.interactable = false;
            ColorBlock cb = kindnessBtn.colors;
            cb.disabledColor = disableColor;
            kindnessBtn.colors = cb;
        }
        if (!TrinaxGlobal.Instance.donateActive)
        {
            generousBtn.interactable = false;
            ColorBlock cb = generousBtn.colors;
            cb.disabledColor = disableColor;
            generousBtn.colors = cb;
        }
    }

    void ToOTP()
    {
        if (TrinaxGlobal.Instance.Dispensed)
        {
            disclaimerText.alpha = 1;
        }
        else
        {
            disclaimerText.alpha = 0;
        }
        OTPEnterBtn.interactable = true;
        OTPResendBtn.interactable = true;
        SetPageActive(PAGES.OTP);
    }

    void OnSubmitOTP()
    {
        bool isValidInput = true;

        if (string.IsNullOrEmpty(OTPIF.text) || OTPIF.text.Length < 6)
        {
            OTPIF.image.DOColor(Color.red, 0.3f);
            OTPIF.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
            {
                OTPEnterBtn.interactable = true;
                OTPResendBtn.interactable = true;
                OTPIF.image.DOColor(Color.white, 0.3f);
            });

            OTPEnterBtn.interactable = false;
            OTPResendBtn.interactable = false;
            isValidInput = false;
        }

        if (isValidInput)
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_SUCCESS);
            OTPEnterBtn.interactable = false;
            OTPResendBtn.interactable = false;

            //TrinaxGlobal.Instance.pOTP = OTPIF.text.Trim();
            //Debug.Log("OTP: " + TrinaxGlobal.Instance.pOTP);


#if !DEBUG_MODE
            //RunCheckOTP().WrapErrors();
#else
            ToMainMenu();
#endif
        }
        else
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
        }
    }

    void OnSubmitDetails()
    {
        bool isValidInput = true;

        //if (string.IsNullOrEmpty(ifMobile.text) || ifMobile.text.Length < 8)
        //{
        //    ifMobile.image.DOColor(Color.red, 0.3f);
        //    ifMobile.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
        //    {
        //        submitEmailBtn.interactable = true;
        //        ifMobile.image.DOColor(Color.white, 0.3f);
        //    });

        //    submitEmailBtn.interactable = false;
        //    isValidInput = false;
        //}

        if (!policyToggle.isOn)
        {
            policyToggle.image.DOColor(Color.red, 0.3f);
            policyToggle.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
            {
                submitEmailBtn.interactable = true;
                policyToggle.image.DOColor(Color.white, 0.3f);
            });

            submitEmailBtn.interactable = false;
            isValidInput = false;
        }

        if (!policy2Toggle.isOn)
        {
            policy2Toggle.image.DOColor(Color.red, 0.3f);
            policy2Toggle.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
            {
                submitEmailBtn.interactable = true;
                policy2Toggle.image.DOColor(Color.white, 0.3f);
            });

            submitEmailBtn.interactable = false;
            isValidInput = false;
        }

        //if (string.IsNullOrEmpty(ifName.text))
        //{
        //    ifName.image.DOColor(Color.red, 0.3f);
        //    ifName.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
        //    {
        //        submitEmailBtn.interactable = true;
        //        ifName.image.DOColor(Color.white, 0.3f);
        //    });

        //    submitEmailBtn.interactable = false;
        //    isValidInput = false;
        //}

        if (string.IsNullOrEmpty(ifEmail.text))
        {
            ifEmail.image.DOColor(Color.red, 0.3f);
            ifEmail.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
            {
                submitEmailBtn.interactable = true;
                ifEmail.image.DOColor(Color.white, 0.3f);
            });

            submitEmailBtn.interactable = false;
            isValidInput = false;
        }
        else
        {
            string str = ifEmail.text.Trim();
            int countSymbol = str.Length - str.Replace("@", "").Length;
            if (countSymbol != 1 || !str.Contains(".") || str.IndexOf('@') == 0 || str.Contains(" "))
            {
                ifEmail.image.DOColor(Color.red, 0.3f);
                ifEmail.transform.DOShakePosition(0.75f, new Vector3(50f, 0, 0), 10, 10, false, true).OnComplete(() =>
                {
                    submitEmailBtn.interactable = true;
                    ifEmail.image.DOColor(Color.white, 0.3f);
                });

                submitEmailBtn.interactable = false;
                isValidInput = false;
            }
        }

        if (isValidInput)
        {
            submitEmailBtn.interactable = false;
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_SUCCESS);
            //TrinaxGlobal.Instance.pName = ifName.text.Trim();
            TrinaxGlobal.Instance.pEmail = ifEmail.text.Trim();
            //TrinaxGlobal.Instance.pMobile = ifMobile.text.Trim();

            Debug.Log(/*"Name: " + TrinaxGlobal.Instance.pName +*/ " | " + "Email: " + TrinaxGlobal.Instance.pEmail + " | " /*+ "Mobile: " + TrinaxGlobal.Instance.pMobile*/);

#if !DEBUG_MODE
            DoRegister().WrapErrors();
#else
            ToOTP();
#endif
        }
        else
        {
            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.FEEDBACK_ERROR);
        }
    }

    void SetPageActive(PAGES page, System.Action callback = null)
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

    bool IsPageActive(PAGES check)
    {
        int index = (int)check;

        if (index < 0 || index > pageList.Length) return false;

        return pageList[index].alpha > 0.5f;
    }

    [ContextMenu("AutoFillForm")]
    void AutoFillForm()
    {
        ifName.text = "aloy";
        ifEmail.text = "aloysius@trinax.sg";
        ifMobile.text = "97736496";
        policyToggle.isOn = true;
        //TrinaxGlobal.Instance.runningActive = false;
        //TrinaxGlobal.Instance.ballActive = false;
        //TrinaxGlobal.Instance.memoryActive = false;
        //TrinaxGlobal.Instance.donateActive = false;
    }

}
