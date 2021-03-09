using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class TrinaxArduino : MonoBehaviour
{
    #region SINGLETON
    public static TrinaxArduino Instance;
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

    public string port1;
    public string port2;

    public string Port1
    {
        get { return TrinaxGlobal.Instance.adnComPort1; }
    }

    public string Port2
    {
        get { return TrinaxGlobal.Instance.adnComPort2; }
    }

    public int baudRate = 9600;

    SerialPort stream;
    SerialPort stream2;

    bool IsSecondArduinoConnected { get { return Port2 != ""; } }

    void Start()
    {
        StartArduinos();
    }

    void StartArduinos()
    {
        try
        {
            stream = new SerialPort(FormatComPort(Port1), baudRate);
            //stream = new SerialPort("\\\\.\\COM11", baudRate);
            stream.ReadTimeout = 10;
            stream.Open();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        try
        {
            if (IsSecondArduinoConnected)
            {
                stream2 = new SerialPort(FormatComPort(Port2), baudRate);
                stream2.ReadTimeout = 10;
                stream2.Open();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        StartCoroutine(AsynchronousReadFromArduino(stream, OnReadSuccess));
        //StartCoroutine(AsynchronousReadFromArduino(stream2, OnReadSuccess));
    }

    string FormatComPort(string target)
    {
        string port = target.Substring(3);
        string actualPortString = "";
        actualPortString = port.Length > 1 ? "\\\\.\\" + target : target;
        return actualPortString;
    }

    void OnReadSuccess(string line)
    {
        if (line == null) return;
        if (line == "") return;

        //Debug.Log("---Arduino---" + line.Trim() + "---");

        line = line.Trim();
        if (line.Length > 0)
        {
            //GameManager.Instance.OnReceivedFromArduinoAccum(line);
        }
    }

    private void OnDisable()
    {
        Close();
    }

    void Close()
    {
        if (stream != null)
        {
            stream.Close();
        }

        if (IsSecondArduinoConnected && stream2 != null)
        {
            stream2.Close();
        }

        StopAllCoroutines();
    }

    public void Restart()
    {
        Close();
        StartArduinos();
    }

    public void WriteToArduino(string message)
    {
        Debug.Log("Writing " + message);

        if (stream2 != null && !stream2.IsOpen)
        {
            return;
        }

        int payload = 0;
        if (int.TryParse(message, out payload))
        {
            if (stream2 != null && stream2.IsOpen)
            {
                stream2.WriteLine(message);
                stream2.BaseStream.Flush();
            }
        }
        else
        {
            Debug.Log("Message is not a int");
        }
    }

    public IEnumerator AsynchronousReadFromArduino(SerialPort strm, Action<string> callback)
    {
        string dataString = null;

        do
        {
            yield return new WaitForSeconds(0.020f);

            try
            {
                dataString = strm.ReadLine();
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield return null;
            }
            //else
            //yield return new WaitForSeconds(0.15f);

        } while (true);
    }
}
