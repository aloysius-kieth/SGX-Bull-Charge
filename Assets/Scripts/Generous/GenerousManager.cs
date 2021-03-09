////#define DEBUG_MODE

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//using TMPro;
//using DG.Tweening;
//using UnityEngine.UI;
//using System.Threading.Tasks;

//public class GenerousManager : MonoBehaviour
//{
//    [Header("Pages")]
//    public CanvasGroup[] pageList;

//    [Header("Buttons")]
//    public Button backBtn;
//    public Button nextBtn;
//    public Button doneBtn;
//    public Button backDonationBtn;

//    [Header("Text")]
//    public TextMeshProUGUI donateAmtText;
//    public TextMeshProUGUI thankyouText;

//    public float IdleInterval { get { return TrinaxGlobal.Instance.idleInterval; } }
//    float idleTimer;

//    public float DonationIdleInterval { get { return TrinaxGlobal.Instance.donationIdleInterval;  } }
//    float donationIdleTimer;

//    int amtDonated;
//    bool pollingMoneyInput = false;
//    bool detectingPollingMoneyInput = false;

//    private void Start()
//    {
//        TrinaxGlobal.Instance.state = GAMESTATES.GENEROUS;
//        InitButtonListeners();

//        donateAmtText.text = "$0.00";
//        thankyouText.text = "";
//        amtDonated = 0;
//        ShowInstructions();

//        RunDisableNoteAcceptor().WrapErrors();
//    }

//    private void Update()
//    {
//        if (IsPageActive(GENEROUS_PAGES.INSTRUCTIONS) || IsPageActive(GENEROUS_PAGES.THANKYOU))
//        {
//            idleTimer += Time.deltaTime;
//            if (idleTimer > IdleInterval)
//            {
//                idleTimer = 0;
//                TrinaxGlobal.Instance.isReturningFromActivity = false;
//                if (isNoteAcceptorEnabled)
//                {
//                    TrinaxGlobal.Instance.firstRun = false;
//                }
//                Debug.Log("Idled for too long!");
//                TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.SCREENSAVER);
//            }
//        }

//        if (IsPageActive(GENEROUS_PAGES.DONATE) /*&& amtDonated != 0*/)
//        {
//            donationIdleTimer += Time.deltaTime;
//            // check idle only if money is given
//            if (donationIdleTimer > DonationIdleInterval)
//            {
//                if (isNoteAcceptorEnabled)
//                {
//                    TrinaxGlobal.Instance.firstRun = false;
//                }

//                donationIdleTimer = 0;
//                TrinaxGlobal.Instance.isReturningFromActivity = false;
//                Debug.Log("Idled for too long!");
//                TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.SCREENSAVER);
//            }

//            if (tempAmt != amtDonated)
//            {
//                donationIdleTimer = 0;
//                //Debug.Log("donation timer reset!");
//            }
//        }

//        if (Input.anyKeyDown)
//        {
//            idleTimer = 0;
//        }

//#if DEBUG_MODE
//        if (Input.GetKeyDown(KeyCode.Q))
//        {
//            amtAdd += 10;
//        }
//#endif

//    }

//    void InitButtonListeners()
//    {
//        backBtn.onClick.AddListener(() =>
//        {
//            if (isNoteAcceptorEnabled)
//            {
//                RunDisableNoteAcceptor().WrapErrors();
//            }

//            TrinaxGlobal.Instance.isReturningFromActivity = true;
//            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
//            TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.SCREENSAVER);
//        });

//        backDonationBtn.onClick.AddListener(() =>
//        {
//            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
//            TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.GENEROUS);
//        });

//        nextBtn.onClick.AddListener(() =>
//        {
//            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
//            ToDonate();

//        });

//        doneBtn.onClick.AddListener(() =>
//        {
//            TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.BUTTON_CLICK);
//            pollingMoneyInput = false;
//            //StopCoroutine("PollMoneyInput");

//#if !DEBUG_MODE
//            // TODO: call api to close note acceptor
//            RunDisableNoteAcceptor().WrapErrors();
//#endif
//            // Store donation amount to be send to server
//            TrinaxGlobal.Instance.donateAmount = amtDonated;

//            StartCoroutine(ToResult());
//        });
//    }

//    Color disableColor = new Color(1, 1, 1, 0.5f);
//    Color enabledColor = new Color(1, 1, 1, 1);
//    IEnumerator PollMoneyInput()
//    {
//        detectingPollingMoneyInput = true;
//        // TODO: call api to poll for new money value every 1 sec
//        while (pollingMoneyInput)
//        {
//            // if there is money given to machine
//            if (amtDonated != 0)
//            {
//                doneBtn.interactable = true;
//                backDonationBtn.interactable = false;
//            }

//            if (amtDonated == 0)
//            {
//                // enable backbtn
//                backDonationBtn.interactable = true;
//            }

//            Debug.Log("amtDonated " + amtDonated);
//            donateAmtText.text = "$" + amtDonated.ToString() + ".00";
//            //Debug.Log("Called: " + amtDonated + " times");

//#if DEBUG_MODE
//            //amtDonated += 10;
//#endif
//            yield return new WaitForSeconds(1);
//            //Debug.Log("Waiting 1 second");

//            Debug.Log("Calling Poll Money Input");
//            yield return RunPollMoneyInputAsync().AsIEnumerator();
//        }
//        detectingPollingMoneyInput = false;
//        //Debug.Log("detecting polling money input " + detectingPollingMoneyInput);
//    }

//    int tempAmt = 0;
//    int amtAdd = 0;
//    async Task RunPollMoneyInputAsync()
//    {
//        if (pollingMoneyInput)
//        {
//#if DEBUG_MODE
//            // store amount 
//            tempAmt = amtDonated;

//            // get amount donated from user
//            amtDonated = amtAdd;
//#else
//            await TrinaxAsyncServerManager.Instance.Donate((bool success, DonateReceiveJsonData rJson) =>
//            {
//                if (success)
//                {
//                    if (rJson.data.result == true)
//                    {
//                        Debug.Log("<color=green>Running poll money input</color>");
//                        // store amount 
//                        tempAmt = amtDonated;
//                        // get amount donated from user
//                        amtDonated = rJson.data.amount;
//                    }
//                    else
//                    {
//                        Debug.Log("<color=red>Polling for money died! Sending to server failed!</color>");
//                    }
//                }
//                else
//                {
//                    Debug.Log("<color=red>Polling for money died! Communication to server failed!</color>");
//                }
//            });
//#endif
//        }
//    }

//    void ShowInstructions()
//    {
//        SetPageActive(GENEROUS_PAGES.INSTRUCTIONS);
//    }

//    void ToDonate()
//    {
//        SetPageActive(GENEROUS_PAGES.DONATE);

//#if !DEBUG_MODE
//        //TODO: call api to activate note accepter
//        RunActivateNoteAcceptor().WrapErrors();
//#endif
//    }

//    bool isNoteAcceptorEnabled = false;
//    async Task RunActivateNoteAcceptor()
//    {
//        while (!isNoteAcceptorEnabled)
//        {
//            await new WaitForSeconds(1);

//            await TrinaxAsyncServerManager.Instance.EnableDonate((bool success, EnableReceiveJsonData rJson) =>
//            {
//                if (success)
//                {
//                    if (rJson.data.result == true)
//                    {
//                        isNoteAcceptorEnabled = true;
//                        isNoteAcceptorDisabled = false;
//                        Debug.Log("Note Acceptor <color=green>ACTIVATED!</color>");
//                        pollingMoneyInput = true;
//                        if (pollingMoneyInput && !detectingPollingMoneyInput)
//                        {
//                            StartCoroutine(PollMoneyInput());
//                        }
//                        else
//                        {
//                            Debug.LogWarning("Polling for money input did not occur!");
//                        }

//                    }
//                    else
//                    {
//                        Debug.Log("Note Acceptor <color=red>NOT ACTIVATED!</color>");
//                        isNoteAcceptorEnabled = false;
//                    }
//                }
//                else
//                {
//                    Debug.Log("Note Acceptor <color=red>NOT ACTIVATED!</color>");
//                    // communication to note acceptor failed, thus call it again to activate
//                    isNoteAcceptorEnabled = false;
//                }
//            });
//        }
//    }

//    bool isNoteAcceptorDisabled = true;
//    async Task RunDisableNoteAcceptor()
//    {
//        while (!isNoteAcceptorDisabled)
//        {
//            await new WaitForSeconds(1);

//            await TrinaxAsyncServerManager.Instance.DisableDonate((bool success, DisableReceiveJsonData rJson) =>
//            {
//                if (success)
//                {
//                    if (rJson.data.result == true)
//                    {
//                        Debug.Log("Note Acceptor <color=green>DISABLED!</color>");
//                        isNoteAcceptorDisabled = true;
//                    }
//                    else
//                    {
//                        Debug.Log("Note Acceptor <color=red>STILL RUNNING! sending to server failed!</color>");
//                        isNoteAcceptorDisabled = false;
//                    }
//                }
//                else
//                {
//                    Debug.Log("Note Acceptor <color=red>STILL RUNNING! communication to server failed!</color>");
//                    isNoteAcceptorDisabled = false;
//                }
//            });
//        }

//    }

//    IEnumerator ToResult()
//    {
//        TrinaxAudioManager.Instance.ImmediateStopMusic();
//        TrinaxAudioManager.Instance.PlayUISFX(TrinaxAudioManager.AUDIOS.APPLAUSE);
//        TrinaxAudioManager.Instance.PlaySFX(TrinaxAudioManager.AUDIOS.THANK_YOU, ()=>
//        {
//            TrinaxAudioManager.Instance.PlayMusic(TrinaxAudioManager.AUDIOS.IDLE, true);
//        });
//        TrinaxGlobal.Instance.donateActive = false;

//        thankyouText.text = "THANK YOU!" + "\n" + "You are awesome";
//        SetPageActive(GENEROUS_PAGES.THANKYOU);

//        yield return new WaitForSeconds(5.0f);
//        ToPhotoTaking();
//    }

//    void ToPhotoTaking()
//    {
//        TrinaxGlobal.Instance.ChangeLevel(GAMESTATES.PHOTOTAKING);
//    }

//    void SetPageActive(GENEROUS_PAGES page, System.Action callback = null)
//    {
//        int pageNum = (int)page;
//        for (int i = 0; i < pageList.Length; i++)
//        {
//            CanvasGroup canvasGrp = pageList[i];
//            // If on current page...
//            if (i == pageNum)
//            {
//                canvasGrp.gameObject.SetActive(true);
//                canvasGrp.blocksRaycasts = true;
//                canvasGrp.DOFade(1.0f, 0.5f).OnComplete(() =>
//                {
//                    callback?.Invoke();
//                });
//            }
//            else
//            {
//                canvasGrp.blocksRaycasts = false;
//                if (canvasGrp.alpha > 0.1f)
//                {
//                    canvasGrp.DOFade(0f, 0.0001f).OnComplete(() =>
//                    {
//                        canvasGrp.gameObject.SetActive(false);
//                    });
//                }
//            }
//        }
//    }

//    public bool IsPageActive(GENEROUS_PAGES check)
//    {
//        int index = (int)check;

//        if (index < 0 || index > pageList.Length) return false;

//        return pageList[index].alpha > 0.5f;
//    }
//}
