#define USE_MOCKY

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrinaxServerManager : MonoBehaviour
{
    #region SINGLETON
    public static TrinaxServerManager Instance { get; set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public static string qrId;
    public static string gameResultId;

    // Change this as when needed
    const string frontUrl = ":8080/northpoint_cny_2018/northpoint.do?";

    public string ip;
    public bool useServer;

    public GameObject loadingCircle;
    bool isLoading = false;
    bool verifying = false;
    bool isDelayedScanCoroutineRunning = false;

    string LOG_DIR = "/log/";
    string LOG_FILE = "server_logs.log";

    public void VerifyQrCode(string qrcode, Action<bool, string> callback)
    {
        if (!useServer)
        {
            callback(true, "TRUE");
            return;
        }

        //if (MainManager.Instance.GetGameState() != GAMESTATES.DEMO)
        //{
        //    Debug.Log("Not at QR verification page!");
        //    return;
        //}

        if (verifying)
        {
            if (isDelayedScanCoroutineRunning)
            {
                Debug.Log("Please wait");
            }
            else
            {
                Debug.Log("Delayed Scan Coroutine not running! Running now.");
                StartCoroutine(DelayQrScanner());
            }
            return;
        }

        verifying = true;
        qrcode = qrcode.Trim();

#if USE_MOCKY
        string url = "http://www.mocky.io/v2/5b078b832f00004f00c620f3"; // redeemed before
        //string url = "http://www.mocky.io/v2/5b078bb92f00005500c620f4"; // invalid
#else
        string url = "http://" + ip + frontUrl + "formType=addInteraction&qrID=" + qrcode;
#endif
        Coroutine call = StartCoroutine(LoadUrl(url, (request) =>
        {
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim().ToUpper();
                if (result == "REDEEMED_BEFORE")
                {
                    callback(false, result);
                }
                else if (result == "INVALID_CODE")
                {
                    callback(false, result);
                }
                else
                {
                    callback(false, "");
                }
            }
            else
            {
                WriteError(request, "VerifyQrCode");
                callback(false, "FALSE");
            }
            //verifying = false;
        }));

        StartCoroutine(DelayQrScanner());
    }

    public void AddGameResult(int score, Action<bool> callback)
    {
        if (!useServer)
        {
            callback(true);
            return;
        }

        string gameResult = score.ToString();

#if USE_MOCKY
        string url = "http://www.mocky.io/v2/5ad0094b3100004d004eab17";
#else
        string url = "http://" + ip + frontUrl + "formType=addGameResult&result=" + gameResult + "&qrID=" + qrId;
#endif
        Coroutine call = StartCoroutine(LoadUrl(url, (request) =>
        {
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim();
                if (!string.IsNullOrEmpty(result))
                {
                    gameResultId = result;
                    callback(true);
                }
                else
                {
                    callback(false);
                }
            }
            else
            {
                WriteError(request, "addGameResult");
                callback(false);
            }
        }));
    }

    public void SendPrint(Action<bool> callback)
    {
        if (!useServer)
        {
            callback(true);
            return;
        }

#if USE_MOCKY
        string url = "http://www.mocky.io/v2/5a38d6d53200007d47eb6dc3";
#else
        string url = "http://" + ip + frontUrl + "formType=sendPrint&qrID=" + qrId;
#endif
        Coroutine call = StartCoroutine(LoadUrl(url, (request) =>
        {
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim();
                if (!string.IsNullOrEmpty(result))
                {
                    bool outcome = false;
                    bool.TryParse(result, out outcome);
                    callback(outcome);
                }
                else
                {
                    callback(false);
                }
            }
            else
            {
                WriteError(request, "sendPrint");
                callback(false);
            }
        }));
    }

    public void GetVoucher(Action<bool, string> callback)
    {
        if (!useServer)
        {
            callback(true, "Mugii");
            return;
        }

#if USE_MOCKY
        string[] urls = {
            "http://www.mocky.io/v2/5a546add2d000003125b1b8f",
            "http://www.mocky.io/v2/5a546b0b2d000001125b1b91",
            "http://www.mocky.io/v2/5a546b1f2d000009125b1b92"
        };
        string url = urls[UnityEngine.Random.Range(0, urls.Length)];
#else
        string url = "http://" + ip + frontUrl + "formType=getVoucher&qrID=" + qrId + "&gameResultID=" + gameResultId;
#endif
        Coroutine call = StartCoroutine(LoadUrl(url, (request) =>
        {
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim();
                if (!string.IsNullOrEmpty(result))
                {
                    callback(true, result);
                }
                else
                {
                    callback(false, "");
                }
            }
            else
            {
                WriteError(request, "getVoucher");
                callback(false, "");
            }
        }));
    }

    public void AddInteraction(Action<bool> callback)
    {
        if (!useServer)
        {
            callback(true);
            return;
        }

#if USE_MOCKY
        string url = "http://www.mocky.io/v2/5a38d6d53200007d47eb6dc3";
#else
        string url = "http://" + ip + frontUrl + "formType=addInteraction";
#endif
        Coroutine call = StartCoroutine(LoadUrl(url, (request) =>
        {
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim();
                if (!string.IsNullOrEmpty(result))
                {
                    bool outcome = false;
                    bool.TryParse(result, out outcome);
                    callback(outcome);
                }
                else
                {
                    callback(false);
                }
            }
            else
            {
                WriteError(request, "addInteraction");
                callback(false);
            }
        }));
    }

    public void SendPhoto(string pName, string pEmail, string typ, Action<bool, string> callback)
    {
        if (!useServer)
        {
            if (typ == "1")
            {
                callback(true, "TRUE");
            }
            else if (typ == "2")
            {
                callback(true, "https://upload.wikimedia.org/wikipedia/commons/7/78/Qrcode_wikipedia_fr_v2clean.png");
            }
            else
            {
                callback(false, "FALSE");
            }
        }

        //if (GameManager.Instance == null) {
        //    WriteError("GameManager is null!", "sendPhoto");
        //    callback(false, "FALSE");
        //    return;
        //}

        // type: 1 or 2 (1: Email, 2: Download)
        string url = "http://" + ip + ":8080/jomalone_2018/jomalone.do?formType=sendPhoto&name=" + pName + "&email=" + pEmail + "&type=" + typ;

        Coroutine call = StartCoroutine(LoadUrl(url, (request) =>
        {
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim();
                if (!string.IsNullOrEmpty(result))
                {
                    callback(true, result);
                }
                else
                {
                    callback(false, "FALSE");
                }
            }
            else
            {
                WriteError(request, "sendPhoto");
                callback(false, "FALSE");
            }
        }));

    }

    void WriteError(WWW request, string api)
    {
        string error = "<" + api + "> --- Begin Error Message: " + request.error + " >> Url: " + request.url + System.Environment.NewLine;
        File.AppendAllText(LOG_DIR + LOG_FILE, error);
    }

    void WriteError(string errorStr, string api)
    {
        string error = "<" + api + "> --- Begin Error Message: " + errorStr + System.Environment.NewLine;
        File.AppendAllText(LOG_DIR + LOG_FILE, error);
    }

    IEnumerator LoadUrl(string url, Action<WWW> callback)
    {
        isLoading = true;

        StartCoroutine(DelayLoadingCircle());

        WWW request = new WWW(url);
        yield return request;

        //yield return new WaitForSeconds(3.0f); // artifical wait for 150ms

        Debug.Log(url + "\n" + request.text);

        callback(request);
        isLoading = false;
        if (loadingCircle != null)
        {
            loadingCircle.SetActive(false);
        }
    }

    IEnumerator DelayLoadingCircle()
    {
        yield return new WaitForSeconds(1.5f);

        if (isLoading && loadingCircle != null)
        {
            loadingCircle.SetActive(true);
        }
    }

    IEnumerator DelayQrScanner()
    {
        isDelayedScanCoroutineRunning = true;
        yield return new WaitForSeconds(3f);

        verifying = false;
        isDelayedScanCoroutineRunning = false;
    }

    private void Start()
    {
        LOG_DIR = Application.dataPath + LOG_DIR;

        if (!Directory.Exists(LOG_DIR))
            Directory.CreateDirectory(LOG_DIR);

        if (loadingCircle != null)
            loadingCircle.SetActive(false);
    }
}
