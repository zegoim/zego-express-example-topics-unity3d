using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZEGO;

public class Setting : MonoBehaviour
{
    ZegoExpressEngine engine;
    private DeviceOrientation preOrientation = DeviceOrientation.Unknown;

    // Start is called before the first frame update
    void Start()
    {
        InitAll();
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID||  UNITY_IPHONE
        if (engine != null)
        {
            if (preOrientation != Input.deviceOrientation)
            {

                if (Input.deviceOrientation == DeviceOrientation.Portrait)
                {
                    engine.SetAppOrientation(ZegoOrientation.ZegoOrientation_0);
                }
                else if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
                {
                    engine.SetAppOrientation(ZegoOrientation.ZegoOrientation_180);
                }
                else if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
                {
                    engine.SetAppOrientation(ZegoOrientation.ZegoOrientation_90);
                }
                else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
                {
                    engine.SetAppOrientation(ZegoOrientation.ZegoOrientation_270);
                }
                preOrientation = Input.deviceOrientation;

            }
        }
#endif
    }

    public void Home()
    {
        // load scene home page
        SceneManager.LoadScene("HomePage");
    }

    void InitAll()
    {
        string appid = GetAppIdConfig.appID.ToString();
        string appSign = GetAppIdConfig.appSign;
        string sdkVersion = string.Format("SDK: {0}", ZegoExpressEngine.GetVersion());

        GameObject.Find("Text_AppID").GetComponent<Text>().text = appid;
        GameObject.Find("Text_AppSign").GetComponent<Text>().text = appSign;
        GameObject.Find("Text_Version").GetComponent<Text>().text = sdkVersion;
        GameObject.Find("InputField_LogPath").GetComponent<InputField>().text = Application.dataPath;
        GameObject.Find("InputField_LogSize").GetComponent<InputField>().text = "5000000";
    }

    public void OnButtonSetLogConfig()
    {
        string logPath = GameObject.Find("InputField_LogPath").GetComponent<InputField>().text;
        ulong logSize = ulong.Parse(GameObject.Find("InputField_LogSize").GetComponent<InputField>().text);
        ZegoLogConfig config = new ZegoLogConfig();
        config.logPath = logPath;
        config.logSize = logSize;

        DestroyEngine();

        ZegoUtilHelper.PrintLogToView(string.Format("SetLogConfig,  logPath:{0}, logSize{1}", logPath, logSize));
        ZegoExpressEngine.SetLogConfig(config);

        CreateEngine();
    }

    void DestroyEngine()
    {
        if(engine != null)
        {
            ZegoUtilHelper.PrintLogToView(string.Format("DestroyEngine"));
            ZegoExpressEngine.DestroyEngine();
            engine = null;
        }
    }

    void CreateEngine()
    {
        if(engine == null)
        {
            ZegoEngineProfile profile = new ZegoEngineProfile();
            profile.appID = GetAppIdConfig.appID;
            profile.appSign = GetAppIdConfig.appSign;
            profile.scenario = ZegoScenario.General;
            ZegoUtilHelper.PrintLogToView(string.Format("CreateEngine, appID:{0}, appSign:{1}, scenerio:{2}", profile.appID, profile.appSign, profile.scenario));
            engine = ZegoExpressEngine.CreateEngine(profile);
        }
    }
}
