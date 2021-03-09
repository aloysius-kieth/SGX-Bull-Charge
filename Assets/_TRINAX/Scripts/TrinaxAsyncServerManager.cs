using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

using System.Threading.Tasks;

/// <summary>
/// Async version of server manager
/// </summary>
public class TrinaxAsyncServerManager : MonoBehaviour
{
    #region SINGLETON
    public static TrinaxAsyncServerManager Instance { get; set; }
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

    public string ip;
    public bool useServer;
    public bool useMocky;
    public GameObject loadingCircle;

    //public string userID;

    const string frontUrl = "northpoint_cny_2018/northpoint.do?";
    const string port = ":8080/";

    const string ERROR_CODE_200 = "200";
    const string ERROR_CODE_201 = "201";

    bool isLoading = false;
    bool isVerifying = false;
    bool isDelayedScanCoroutineRunning = false;

    string LOG_DIR = "/log/";
    string LOG_FILE = "Async_server_logs.log";

    #region API Funcs
    public async Task AddInteractionAsync(Action<bool> callback)
    {
        if (!useServer)
        {
            callback(true);
            return;
        }

        string url;

        if (!useMocky)
        {
            Debug.Log("Using actual server url...");
            url = "http://www.mocky.io/v2/5b4cb3f9310000472aa7e134";
            //url = "http://" + ip + port + frontUrl + "formType=addInteraction";
        }
        else
        {
            Debug.Log("Using mocky url...");
            url = "http://www.mocky.io/v2/5b6122fb3000007f006a3ffc";
        }

        var r = await LoadUrlAsync(url, (request) =>
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
                WriteError(request, "AddInteractionAsync");
                callback(false);
            }
        });

        Debug.Log(r);
    }

    const string QR_REDEEMED_BEFORE = "REDEEMED_BEFORE";
    const string QR_INVALID_CODE = "INVALID_CODE";
    public async Task VerifyQrCodeAsync(string qrCode, Action<bool, string> callback)
    {
        if (!useServer)
        {
            callback(true, "TRUE");
            return;
        }

        // TODO: Check if main manager is at a certain point in the game for this API to be called

        if(isVerifying)
        {
            if (isDelayedScanCoroutineRunning)
            {
                Debug.Log("Already scanning!");
            }
            else
            {
                Debug.Log("Running delay qr scanner task");
                DelayQrScanner();
            }
            return;
        }

        isVerifying = true;
        // Remove excess whitespaces
        qrCode = qrCode.Trim();
        string url;

        if (!useMocky)
        {
            Debug.Log("Using actual server url...");
            url = "http://www.mocky.io/v2/5b50af743600003c0ddd0f98";
            //url = "http://" + ip + port + frontUrl + "formType=addInteraction&qrID=" + qrCode;
        }
        else
        {
            Debug.Log("Using mocky url...");
            // returns ID
            //url = "http://www.mocky.io/v2/5b50af743600003c0ddd0f98";
            // returns REDEEMED_BEFORE
            url = "http://www.mocky.io/v2/5b50afc73600007d00dd0f9b";
        }

        var r = await LoadUrlAsync(url, (request) =>
        {
            if(string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim().ToUpper();
                if(!string.IsNullOrEmpty(result))
                {
                    if(result == QR_REDEEMED_BEFORE)
                    {
                        callback(false, result);
                    }
                    else if (result == QR_INVALID_CODE)
                    {
                        callback(false, result);
                    }
                    else
                    {
                        callback(true, "TRUE");
                    }
                }
            }
            else
            {
                WriteError(request, "VerifyQrCodeAsync");
                callback(false, "FALSE");
            }
        });

        DelayQrScanner();
        Debug.Log(r);
    }

    public async Task Register(RegisterSendJsonData json, Action<bool, RegisterReceiveJsonData> callback)
    {
        RegisterReceiveJsonData data;
        if (!useServer)
        {
            RegisterData registerData = new RegisterData
            {
                userID = "4562018091817094516460954353864869630193245-6916-4aee-b6a2-e956977aa17a",
                dispense = false,
            };

            data = new RegisterReceiveJsonData
            {
                data = registerData,
            };
            callback(true, data);
            return;
        }

        string sendJson = JsonUtility.ToJson(json);

        string url;
        if (!useMocky)
        {
            Debug.Log("Using server url...");
            url = "http://" + ip + port + "SGX/Servlet/ApiNew.do/register";
        }
        else
        {
            Debug.Log("Using mocky url...");
            // False
            //url = "http://www.mocky.io/v2/5ba1b40a35000062005bbe0b";
            // True
            url = "http://www.mocky.io/v2/5ba1d12035000068005bbe56";
        }

        var r = await LoadPostUrl(url, sendJson, (request) =>
        {
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim();
                data = JsonUtility.FromJson<RegisterReceiveJsonData>(result);
                //Debug.Log("myresult: " + result);

                if (data.error.error_code == ERROR_CODE_200)
                {
                    // Get userID from server
                    //userID = data.data.id;
                    //TrinaxGlobal.Instance.userType = data.data.userType;
                    //TrinaxGlobal.Instance.runningActive = data.data.r001;
                    //TrinaxGlobal.Instance.ballActive = data.data.r002;
                    //TrinaxGlobal.Instance.memoryActive = data.data.r003;
                    //TrinaxGlobal.Instance.donateActive = data.data.r004;
                    callback(true, data);
                }
                else
                {
                    callback(false, data);
                }
            }
            else
            {
                WriteError(request, "Register");
                data = new RegisterReceiveJsonData();
                callback(false, data);
            }
        });
    }

    //public async Task CheckOTP(Action<bool, OTPRecieveJsonData> callback)
    //{
    //    OTPRecieveJsonData data;
    //    if (!useServer)
    //    {
    //        data = new OTPRecieveJsonData
    //        {
    //            data = true,
    //        };
    //    }

    //    string url;
    //    if (!useMocky)
    //    {
    //        url = "http://" + ip + port + "SGX/Servlet/ApiNew.do/checkOTP/" + TrinaxGlobal.Instance.pUserId + "/" + TrinaxGlobal.Instance.pOTP;
    //    }
    //    else
    //    {
    //        url = "http://www.mocky.io/v2/5ba1b5a835000069005bbe0c";
    //    }

    //    var r = await LoadUrlAsync(url, (request) =>
    //    {
    //        if (string.IsNullOrEmpty(request.error))
    //        {
    //            string result = request.text.Trim();
    //            data = JsonUtility.FromJson<OTPRecieveJsonData>(result);

    //            if (!string.IsNullOrEmpty(result))
    //            {
    //                if (data.error.error_code == ERROR_CODE_200)
    //                {
    //                    callback(true, data);
    //                }
    //                else if ((data.error.error_code == ERROR_CODE_201))
    //                {
    //                    callback(true, data);
    //                }
    //                else
    //                {
    //                    callback(false, data);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            WriteError(request, "CheckOTP`");
    //            data = new OTPRecieveJsonData();
    //            callback(false, data);
    //        }
    //    });
    //}

    public async Task AddResult(AddResultSendJsonData json, Action<bool, AddResultReceiveJsonData> callback)
    {
        AddResultReceiveJsonData data;
        if (!useServer)
        {
            data = new AddResultReceiveJsonData();
            callback(true, data);
            return;
        }

        string sendJson = JsonUtility.ToJson(json);

        string url;
        if (!useMocky)
        {
            url = "http://" + ip + port + "SGX/Servlet/Function.do/addResult";
        }
        else
        {
            url = "http://www.mocky.io/v2/5ba1b7e03500005e005bbe14";
        }

        var r = await LoadPostUrl(url, sendJson, (request) =>
        {
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim();
                data = JsonUtility.FromJson<AddResultReceiveJsonData>(result);
                //Debug.Log("myresult: " + result);

                if (data.error.error_code == ERROR_CODE_200)
                {
                    callback(true, data);
                }
                else
                {
                    callback(false, data);
                }
            }
            else
            {
                WriteError(request, "AddResult");
                data = new AddResultReceiveJsonData();
                callback(false, data);
            }
        });
    }

    public async Task Dispense(DispenseSendJsonData json, Action<bool, DispenseReceiveJsonData> callback)
    {
        DispenseReceiveJsonData data;
        if (!useServer)
        {
            data = new DispenseReceiveJsonData();
            callback(true, data);
            return;
        }

        string sendJson = JsonUtility.ToJson(json);

        string url;
        if (!useMocky)
        {
            url = "http://" + ip + port + "SGX/Servlet/VendingTransaction.do/dispense";
        }
        else
        {
            Debug.Log("No url to load!");
            return;
        }

        var r = await LoadPostUrl(url, sendJson, (request) =>
        {
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim();
                data = JsonUtility.FromJson<DispenseReceiveJsonData>(result);
                //Debug.Log("myresult: " + result);

                if (data.error.error_code == ERROR_CODE_200)
                {
                    callback(true, data);
                }
                else
                {
                    callback(false, data);
                }
            }
            else
            {
                WriteError(request, "Dispense");
                data = new DispenseReceiveJsonData();
                callback(false, data);
            }
        });
    }

    //// GET URL
    //public async Task EnableDonate(Action<bool, EnableReceiveJsonData>callback)
    //{
    //    EnableReceiveJsonData data;
    //    if (!useServer)
    //    {
    //        data = new EnableReceiveJsonData();
    //        callback(true, data);
    //        return;
    //    }

    //    string url;
    //    if (!useMocky)
    //    {
    //        url = "http://" + ip + port + "SGX/Servlet/Donation.do/enable";
    //    }
    //    else
    //    {
    //        Debug.Log("No url to load!");
    //        return;
    //    }

    //    var r = await LoadUrlAsync(url, (request) =>
    //    {
    //        if (string.IsNullOrEmpty(request.error))
    //        {
    //            string result = request.text.Trim();
    //            data = JsonUtility.FromJson<EnableReceiveJsonData>(result);
    //            //Debug.Log("myresult: " + result);

    //            if (data.error.error_code == ERROR_CODE_200)
    //            {
    //                Debug.Log("EnableDonate success!");
    //                callback(true, data);
    //            }
    //            else
    //            {
    //                Debug.Log("EnableDonate fail!");
    //                callback(false, data);
    //            }
    //        }
    //        else
    //        {
    //            WriteError(request, "EnableDonate");
    //            data = new EnableReceiveJsonData();
    //            callback(false, data);
    //        }
    //    });
    //}

    //// GET URL
    //public async Task DisableDonate(Action<bool, DisableReceiveJsonData> callback)
    //{
    //    DisableReceiveJsonData data;
    //    if (!useServer)
    //    {
    //        data = new DisableReceiveJsonData();
    //        callback(true, data);
    //        return;
    //    }

    //    string url;
    //    if (!useMocky)
    //    {
    //        url = "http://" + ip + port + "SGX/Servlet/Donation.do/disable";
    //    }
    //    else
    //    {
    //        Debug.Log("No url to load!");
    //        return;
    //    }

    //    var r = await LoadUrlAsync(url, (request) =>
    //    {
    //        if (string.IsNullOrEmpty(request.error))
    //        {
    //            string result = request.text.Trim();
    //            data = JsonUtility.FromJson<DisableReceiveJsonData>(result);
    //            //Debug.Log("myresult: " + result);

    //            if (data.error.error_code == ERROR_CODE_200)
    //            {
    //                callback(true, data);
    //            }
    //            else
    //            {
    //                callback(false, data);
    //            }
    //        }
    //        else
    //        {
    //            WriteError(request, "DisableDonate");
    //            data = new DisableReceiveJsonData();
    //            callback(false, data);
    //        }
    //    });
    //}

    //// GET URL
    //public async Task Donate(Action<bool, DonateReceiveJsonData> callback)
    //{
    //    DonateReceiveJsonData data;
    //    if (!useServer)
    //    {
    //        data = new DonateReceiveJsonData();
    //        callback(true, data);
    //        return;
    //    }

    //    string url;
    //    if (!useMocky)
    //    {
    //        url = "http://" + ip + port + "SGX/Servlet/Donation.do/donate";
    //    }
    //    else
    //    {
    //        Debug.Log("No url to load!");
    //        return;
    //    }

    //    var r = await LoadUrlAsync(url, (request) =>
    //    {
    //        if (string.IsNullOrEmpty(request.error))
    //        {
    //            string result = request.text.Trim();
    //            data = JsonUtility.FromJson<DonateReceiveJsonData>(result);
    //            //Debug.Log("myresult: " + result);

    //            if (data.error.error_code == ERROR_CODE_200)
    //            {
    //                callback(true, data);
    //            }
    //            else
    //            {
    //                callback(false, data);
    //            }
    //        }
    //        else
    //        {
    //            WriteError(request, "Donate");
    //            data = new DonateReceiveJsonData();
    //            callback(false, data);
    //        }
    //    });
    //}

    // GET URL
    public async Task CheckStock(Action<bool, StockCheckReceiveJsonData> callback)
    {
        StockCheckReceiveJsonData data;
        if (!useServer)
        {
            //StockCheckData checkData = new StockCheckData
            //{
            //    total = 100,
            //};
            data = new StockCheckReceiveJsonData {
                total = 100,
            };

            callback(true, data);
            return;
        }

        string url;
        if (!useMocky)
        {
            url = "http://" + ip + port + "SGX/Servlet/Vending.do/vendingProducts/";/* + TrinaxGlobal.Instance.gameType;*/
        }
        else
        {
            // GOT STOCK
            url = "http://www.mocky.io/v2/5ba1b6f635000065005bbe12";
            // NO STOCK
            //url = "http://www.mocky.io/v2/5ba1c66f35000057005bbe36";
        }

        var r = await LoadUrlAsync(url, (request) =>
        {
            if (string.IsNullOrEmpty(request.error))
            {
                string result = request.text.Trim();
                data = JsonUtility.FromJson<StockCheckReceiveJsonData>(result);

                if (data.error.error_code == ERROR_CODE_200)
                {
                    callback(true, data);
                }
                else
                {
                    callback(false, data);
                }
            }
            else
            {
                WriteError(request, "CheckStock");
                data = new StockCheckReceiveJsonData();
                callback(false, data);
            }
        });
    }

    #endregion

    async Task<string> LoadUrlAsync(string url, Action<WWW> callback)
    {
        isLoading = true;

        DelayLoadingCircle();

        Debug.Log("Loading url: " + url);
        var request = await new WWW(url);
        Debug.Log(url + "\n" + request.text);

        //await new WaitForSeconds(3.0f); // artifical wait
        callback(request);

        isLoading = false;
        if (loadingCircle != null)
        {
            loadingCircle.SetActive(false);
        }

        return request.text;
    }

    async Task<string> LoadPostUrl(string url, string jsonString, Action<WWW> callback)
    {
        isLoading = true;
        //then set the headers Dictionary headers=form.headers; headers["Content-Type"]="application/json";

        DelayLoadingCircle();

        WWWForm form = new WWWForm();
        byte[] jsonSenddata = null;
        if (!string.IsNullOrEmpty(jsonString))
        {
            Debug.Log(jsonString);
            jsonSenddata = System.Text.Encoding.UTF8.GetBytes(jsonString);
        }

        form.headers["Content-Type"] = "application/json";
        form.headers["Accept"] = "application/json";
        Dictionary<string, string> headers = form.headers;
        headers["Content-Type"] = "application/json";

        Debug.Log("Loading url: " + url);
        var request = await new WWW(url, jsonSenddata, headers);
        Debug.Log(url + "\n" + request.text);


        await new WaitForSeconds(0.1f); // artifical wait for 150ms

        callback(request);

        isLoading = false;
        if (loadingCircle != null)
        {
            loadingCircle.SetActive(false);
        }

        return request.text;
    }

    async void DelayLoadingCircle()
    { 
        await new WaitForSeconds(1.5f);

        if (isLoading && loadingCircle != null)
            loadingCircle.SetActive(true);
    }

    async void DelayQrScanner()
    {
        isDelayedScanCoroutineRunning = true;
        await new WaitForSeconds(3f);

        isVerifying = false;
        isDelayedScanCoroutineRunning = false;
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

    private void Start()
    {
        LOG_DIR = Application.dataPath + LOG_DIR;

        if (!Directory.Exists(LOG_DIR))
            Directory.CreateDirectory(LOG_DIR);

        if (loadingCircle != null)
            loadingCircle.SetActive(false);
    }
}
