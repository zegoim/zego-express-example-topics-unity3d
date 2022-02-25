using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
public class ZegoUtilHelper{
    public static void PrintLogToView()
    {

    }

    public static string DeviceName()
    {
        string device = "";

#if UNITY_ANDROID
        device = "Android";
#elif UNITY_IPHONE
        device = "iPhone";
#elif UNITY_STANDALONE_WIN
        device = "Windows";
#elif UNITY_STANDALONE_LINUX
        device = "Linux";
#elif UNITY_STANDALONE_OSX
        device = "macOS";
#else
        device = "Unknown";
#endif
        return device;
    }

    public string GetRandomString(int min = 0, int max = 99999)
    {
        return UnityEngine.Random.Range(0,99999).ToString();
    }

    public static string UserName()
    {
        return DeviceName() + "_" + System.Environment.UserName + "_" + UnityEngine.Random.Range(0,99999).ToString();
    }

    public static void PrintLogToView(string logInfo)
    {
        // console log
        Debug.Log(logInfo);

        // view log
        GameObject logObj = GameObject.Find("Text_Log");
        if(logObj)
        {
            Text logText = logObj.GetComponent<Text>();
            if(logText != null)
            {
                string time = string.Format("[ {0} ] ", DateTime.Now.ToString("HH:mm:ss.fff"));
                if(logText.text == "")
                {
                    logText.text = time + logInfo;
                }
                else
                {
                    logText.text = logText.text + Environment.NewLine + time + logInfo;
                }
            }
        }
    }
}