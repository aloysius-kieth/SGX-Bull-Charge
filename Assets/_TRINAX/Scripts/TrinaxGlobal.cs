using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;
using TMPro;

public class TrinaxGlobal : MonoBehaviour
{
    #region SINGLETON
    public static TrinaxGlobal Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        //firstRun = true;
    }
    #endregion

    public bool IsDevMode { get; set; }
    public bool IsGameOver { get; set; }
    public bool isReturningFromActivity = false;
    //public bool firstRun = true;

    public string photoPath;
    public string photoFileName = "\\tmp\\tmp.jpg";

    // User info
    public string pUserId { get; set; }
    //public string pName { get; set; }
    public string pEmail { get; set; }
    //public string pMobile { get; set; }
    //public string pOTP { get; set; }
    public bool Dispensed { get; set; }

    // Offsets for ball game
    public float hand_factorX = 3.8f;
    public float hand_factorY = 3.1f;
    public float hand_factorZ = 1.3f;
    public float hand_offsetY = 0f;

    public float maxUserDistance = 2.2f;
    public float MIN_DISTANCE_FROM_KINECT = 1.7f;

    public float idleInterval = 60f;
    public float donationIdleInterval = 180f;

    public int ballGameDuration = 15;
    public int memoryGameDuration = 20;
    public int runningGameDuration = 20;

    public float ballSpawnInterval = 1f;
    public float runningPower = 0.002f;

    public bool runningActive = true;
    public bool ballActive = true;
    public bool memoryActive = true;
    public bool donateActive = true;

    public int donateAmount;

    public const string RUNNING_GAMETYPE = "R-001";
    public const string BALL_GAMETYPE = "R-002";
    public const string MEMORY_GAMETYPE = "R-003";
    public const string DONATE_GAMETYPE = "R-004";

    public string gameType;
    // 1 is staff, 3 is visitor
    public int userType;

    // Resets all activities' booleans (Do at screensaver)
    public void ResetAllGlobalValues()
    {
        runningActive = true;
        ballActive = true;
        memoryActive = true;
        donateActive = true;

        donateAmount = 0;
        gameType = "";
    }

    [Header("Arduino Ports")]
    public string adnComPort1;
    public string adnComPort2;

    [Header("Game State")]
    public GAMESTATES state;

    TrinaxAdminPanel aP;

    public const string SUCCESS_FEEDBACK = "SUCCESSFUL!";
    public const string FAIL_FEEDBACK = "We're experiencing some technical difficulties." + "\n" + "Please try again";
    public const string OUT_OF_STOCK_FEEDBACK = "We are currently out of gifts!" + "\n" + "Sorry for the inconvenience";

    private void Start()
    {
#if !UNITY_EDITOR
        Cursor.visible = false;
#endif

        TrinaxSaveManager.LoadJson();
        aP = TrinaxCanvas.Instance.adminPanel;

        IsGameOver = true;

        if (string.IsNullOrEmpty(TrinaxSaveManager.saveObj.ip.Trim()) || string.IsNullOrEmpty(TrinaxSaveManager.saveObj.photoPath.Trim()))
        {
            Debug.Log("Mandatory fields in admin panel not filled!" + "\n" + "Opening admin panel...");
            aP.gameObject.SetActive(true);
        }
        else
        {
            aP.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Time.frameCount % 30 == 0)
        {
            System.GC.Collect();
        }

        if (aP.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ChangeLevel(GAMESTATES.SCREENSAVER);
            }
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            Cursor.visible = !Cursor.visible;
        }
    }

    public IEnumerator ShowFeedbackMsg(TextMeshProUGUI text, string msg, float fadeInDuration, float fadeOutDuration, System.Action callback = null)
    {
        text.alpha = 0;

        text.text = msg;
        yield return new WaitForSeconds(fadeInDuration);
        text.DOFade(1.0f, 0.25f);
        yield return new WaitForSeconds(fadeOutDuration);
        text.DOFade(0.0f, 0.25f).OnComplete(() =>
        {
             callback?.Invoke();
        });
    }

    public void ChangeLevel(GAMESTATES sceneIndex, TweenCallback callback = null)
    {
        int index = (int)sceneIndex;
        TrinaxLoadSceneAsync.Instance.LoadLevel(index, callback);
    }

    public string FloatToTime(float toConvert, string format)
    {
        switch (format)
        {
            case "00.0":
                return string.Format("{0:00}:{1:0}",
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 10) % 10));//miliseconds
                break;
            case "#0.0":
                return string.Format("{0:#0}:{1:0}",
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 10) % 10));//miliseconds
                break;
            case "00.00":
                return string.Format("{0:00}:{1:00}",
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 100) % 100));//miliseconds
                break;
            case "00.000":
                return string.Format("{0:00}:{1:000}",
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 1000) % 1000));//miliseconds
                break;
            case "#00.000":
                return string.Format("{0:#00}:{1:000}",
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 1000) % 1000));//miliseconds
                break;
            case "#0:00":
                return string.Format("{0:#0}:{1:00}",
                    Mathf.Floor(toConvert / 60),//minutes
                    Mathf.Floor(toConvert) % 60);//seconds
                break;
            case "#00:00":
                return string.Format("{0:#00}:{1:00}",
                    Mathf.Floor(toConvert / 60),//minutes
                    Mathf.Floor(toConvert) % 60);//seconds
                break;
            case "0:00.0":
                return string.Format("{0:0}:{1:00}.{2:0}",
                    Mathf.Floor(toConvert / 60),//minutes
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 10) % 10));//miliseconds
                break;
            case "#0:00.0":
                return string.Format("{0:#0}:{1:00}.{2:0}",
                    Mathf.Floor(toConvert / 60),//minutes
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 10) % 10));//miliseconds
                break;
            case "0:00.00":
                return string.Format("{0:0}:{1:00}.{2:00}",
                    Mathf.Floor(toConvert / 60),//minutes
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 100) % 100));//miliseconds
                break;
            case "#0:00.00":
                return string.Format("{0:#0}:{1:00}.{2:00}",
                    Mathf.Floor(toConvert / 60),//minutes
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 100) % 100));//miliseconds
                break;
            case "0:00.000":
                return string.Format("{0:0}:{1:00}.{2:000}",
                    Mathf.Floor(toConvert / 60),//minutes
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 1000) % 1000));//miliseconds
                break;
            case "#0:00.000":
                return string.Format("{0:#0}:{1:00}.{2:000}",
                    Mathf.Floor(toConvert / 60),//minutes
                    Mathf.Floor(toConvert) % 60,//seconds
                    Mathf.Floor((toConvert * 1000) % 1000));//miliseconds
                break;
        }
        return "error";
    }

}
