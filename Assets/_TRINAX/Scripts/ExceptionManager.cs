using UnityEngine;
using System.IO;

public class ExceptionManager : MonoBehaviour
{
    string LOG_FILE = "exception_logs.log";
    string LOG_DIR = "/log/";

    void Awake()
    {
        Application.logMessageReceived += HandleException;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LOG_DIR = Application.dataPath + LOG_DIR;

        if (!Directory.Exists(LOG_DIR))
            Directory.CreateDirectory(LOG_DIR);

    }

    void HandleException(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            //handle here
            //Debug.Log(string.Format("StackTrace: {0}", stackTrace));
            //Debug.Log(string.Format("Condition: {0}", condition));
            string error = "<" + condition + "> --- Begin Error Message: " + stackTrace + System.Environment.NewLine;
            File.WriteAllText(LOG_DIR + LOG_FILE, error);
        }
    }
}