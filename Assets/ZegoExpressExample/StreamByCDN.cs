using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZEGO;

public class StreamByCDN : MonoBehaviour
{
    ZegoExpressEngine engine;
    ZegoUser user = new ZegoUser();
    string roomID;
    string publishStreamID;
    string playStreamID;
    RawImageVideoSurface localVideoSurface = null;
    RawImageVideoSurface remoteVideoSurface = null;
    private DeviceOrientation preOrientation = DeviceOrientation.Unknown;
    ArrayList cameraList = new ArrayList();
    ArrayList microphoneList = new ArrayList();
    Dropdown mirrorModeDropDown;
    Text text_RoomState;
    Text text_RoomID;
    bool isLoginRoom = false;
    bool isPublish1 = false;
    bool isPublish2 = false;
    bool isPlay = false;
    string publishCdnUrl;
    string playCdnUrl;
    bool isAddCdnUrl = false;

    // Start is called before the first frame update
    void Start()
    {
        CreateEngine();

        InitAll();

        LoginRoom();
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

    void OnDestroy()
    {
        Debug.Log("DestroyEngine");
        ZegoExpressEngine.DestroyEngine();
    }

    void InitAll()
    {
        user.userID = ZegoUtilHelper.UserName();
        user.userName = user.userID;

        roomID = "0010";
        publishStreamID = "0010";
        playStreamID = "0010";

        GameObject previewObj = GameObject.Find("RawImage_Preview");
        if(previewObj != null)
        {
            localVideoSurface = previewObj.AddComponent<RawImageVideoSurface>();
            localVideoSurface.SetCaptureVideoInfo();
            localVideoSurface.SetVideoSource(engine);
        }

        GameObject remoteVideoPlane = GameObject.Find("RawImage_Play");
        if (remoteVideoPlane != null)
        {
            if (remoteVideoSurface == null)//Avoid repeated Add Component causing strange problems such as video freeze
            {
                remoteVideoSurface = remoteVideoPlane.AddComponent<RawImageVideoSurface>();
                remoteVideoSurface.SetPlayVideoInfo(playStreamID);
                remoteVideoSurface.SetVideoSource(engine);
            }
        }

        text_RoomState = GameObject.Find("Text_RoomState").GetComponent<Text>();
        text_RoomID = GameObject.Find("Text_RoomID").GetComponent<Text>();
    }

    void BindEventHandler()
    {
        engine.onRoomStateUpdate = OnRoomStateUpdate;
        engine.onRoomUserUpdate = OnRoomUserUpdate;
        engine.onPublisherStateUpdate = OnPublisherStateUpdate;
        engine.onPlayerStateUpdate = OnPlayerStateUpdate;
        engine.onLocalDeviceExceptionOccurred = OnLocalDeviceExceptionOccurred;
        engine.onDebugError = OnDebugError;
    }

    void OnRoomStateUpdate(string roomID, ZegoRoomState state, int errorCode, string extendedData)
    {
        ZegoUtilHelper.PrintLogToView(string.Format("OnRoomStateUpdate, roomID:{0}, state:{1}, errorCode:{2}, extendedData:{3}", roomID, state, errorCode, extendedData));
        text_RoomState.text = state.ToString();
        text_RoomID.text = roomID;
    }

    void OnRoomUserUpdate(string roomID, ZegoUpdateType updateType, List<ZegoUser> userList, uint userCount)
    {
        if(updateType == ZegoUpdateType.Add)
        {
            userList.ForEach((user)=>{
                ZegoUtilHelper.PrintLogToView(string.Format("user {0} enter room {1}", user.userID, roomID));
            });
        }
        else
        {
            userList.ForEach((user)=>{
                ZegoUtilHelper.PrintLogToView(string.Format("user {0} exit room {1}", user.userID, roomID));
            });
        }
    }

    void OnPublisherStateUpdate(string streamID, ZegoPublisherState state, int errorCode, string extendedData)
    {
        ZegoUtilHelper.PrintLogToView(string.Format("OnPublisherStateUpdate, streamID:{0}, state:{1}, errorCode:{2}, extendedData:{3}", streamID, state, errorCode, extendedData));
    }

    void OnPlayerStateUpdate(string streamID, ZegoPlayerState state, int errorCode, string extendedData)
    {
        ZegoUtilHelper.PrintLogToView(string.Format("OnPlayerStateUpdate, streamID:{0}, state:{1}, errorCode:{2}, extendedData:{3}", streamID, state, errorCode, extendedData));
    }

    void OnLocalDeviceExceptionOccurred(ZegoDeviceExceptionType exceptionType, ZegoDeviceType deviceType, string deviceID)
    {
        ZegoUtilHelper.PrintLogToView(string.Format("OnLocalDeviceExceptionOccurred, exceptionType:{0}, deviceType:{1}, deviceID:{2}", exceptionType, deviceType, deviceID));
    }

    void OnDebugError(int errorCode, string funcName, string info)
    {
        ZegoUtilHelper.PrintLogToView(string.Format("OnDebugError, funcName:{0}, info:{1}", errorCode, funcName, info));
    }

    public void CreateEngine()
    {
        if(engine == null)
        {
            ZegoEngineProfile profile = new ZegoEngineProfile();
            profile.appID = GetAppIdConfig.appID;
            profile.appSign = GetAppIdConfig.appSign;
            profile.scenario = ZegoScenario.General;
            ZegoUtilHelper.PrintLogToView(string.Format("CreateEngine, appID:{0}, appSign:{1}, scenerio:{2}", profile.appID, profile.appSign, profile.scenario));
            engine = ZegoExpressEngine.CreateEngine(profile);
            BindEventHandler();
        }
    }

    public void DestroyEngine()
    {
        ZegoUtilHelper.PrintLogToView("DestroyEngine");
        ZegoExpressEngine.DestroyEngine();
        engine = null;
    }

    void LoginRoom()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("LoginRoom, roomID:{0}, userID:{1}, userName:{2}", roomID, user.userID, user.userName));
        engine.LoginRoom(roomID, user);

        //GameObject.Find("Button_LoginRoom").GetComponent<Button>().GetComponentInChildren<Text>().text = "Logout Room";
    }

    void LogoutRoom()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("LogoutRoom"));
        engine.LogoutRoom();

        //GameObject.Find("Button_LoginRoom").GetComponent<Button>().GetComponentInChildren<Text>().text = "Login Room";
    }

    void StartPreview()
    {
        ZegoUtilHelper.PrintLogToView("StartPreview");
        engine.StartPreview();
    }

    void StopPreview()
    {
        ZegoUtilHelper.PrintLogToView("StopPreview");
        engine.StopPreview();
    }

    void StartPublishingStream()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("StartPublishingStream, streamID:{0}", publishStreamID));
        engine.StartPublishingStream(publishStreamID);
    }

    void StopPublishingStream()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("StopPublishingStream"));
        engine.StopPublishingStream();
    }

    void StartPlaying()
    {  
        ZegoUtilHelper.PrintLogToView(string.Format("StartPlayingStream, streamID:{0}", playStreamID));
        ZegoPlayerConfig config = new ZegoPlayerConfig();
        config.cdnConfig = new ZegoCDNConfig();
        config.cdnConfig.url = GameObject.Find("InputField_PlayCdnUrl").GetComponent<InputField>().text;;
        config.resourceMode = ZegoStreamResourceMode.OnlyCDN;
        engine.StartPlayingStream(playStreamID, config);

        GameObject.Find("Button_StartPlaying").GetComponent<Button>().GetComponentInChildren<Text>().text = "Stop Playing";
    }

    void StopPlaying()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("StopPlayingStream, streamID:{0}", playStreamID));
        engine.StopPlayingStream(playStreamID);

        GameObject.Find("Button_StartPlaying").GetComponent<Button>().GetComponentInChildren<Text>().text = "Start Playing";
    }

    public void OnButtonStartPublishing1()
    {
        ZegoCDNConfig cdnConfig = new ZegoCDNConfig();
        ZegoUtilHelper.PrintLogToView(string.Format("EnablePublishDirectToCDN, enable: false"));
        engine.EnablePublishDirectToCDN(false, cdnConfig);

        // Stop publishing if need
        if(isPublish2)
        {
            isPublish2 = false;
            StopPreview();
            StopPublishingStream();
            GameObject.Find("Button_StartPublishing2").GetComponent<Button>().GetComponentInChildren<Text>().text = "Start Publishing";
        }

        if(isPublish1)
        {
            isPublish1 = false;
            StopPreview();
            StopPublishingStream();
            GameObject.Find("Button_StartPublishing1").GetComponent<Button>().GetComponentInChildren<Text>().text = "Start Publishing";
        }
        else
        {
            isPublish1 = true;
            StartPreview();
            StartPublishingStream();
            GameObject.Find("Button_StartPublishing1").GetComponent<Button>().GetComponentInChildren<Text>().text = "Stop Publishing";
        }
    }

    public void OnButtonStartPublishing2()
    {
        // Stop publishing if need
        if(isPublish1)
        {
            isPublish1 = false;
            StopPreview();
            StopPublishingStream();
            GameObject.Find("Button_StartPublishing1").GetComponent<Button>().GetComponentInChildren<Text>().text = "Start Publishing";
        }

        if(isPublish2)
        {
            isPublish2 = false;
            StopPreview();
            StopPublishingStream();
            GameObject.Find("Button_StartPublishing2").GetComponent<Button>().GetComponentInChildren<Text>().text = "Start Publishing";
        }
        else
        {
            isPublish2 = true;
            StartPreview();
            StartPublishingStream();
            GameObject.Find("Button_StartPublishing2").GetComponent<Button>().GetComponentInChildren<Text>().text = "Stop Publishing";
        }
    }

    public void OnToggleEnablePublishDirectToCDN()
    {
        bool enable = GameObject.Find("Toggle_EnablePublishDirectToCDN").GetComponent<Toggle>().isOn;
        string url = GameObject.Find("InputField_PubishCdnUrl2").GetComponent<InputField>().text;
        ZegoCDNConfig cdnConfig = new ZegoCDNConfig();
        cdnConfig.url = url;
        ZegoUtilHelper.PrintLogToView(string.Format("EnablePublishDirectToCDN, enable: {0}, url:{1}", enable, url));
        engine.EnablePublishDirectToCDN(enable, cdnConfig);
    }

    public void OnButtonStartPlaying()
    {
        if(isPlay)
        {
            isPlay = false;
            StopPlaying();
        }
        else
        {
            isPlay = true;
            StartPlaying();
        }
    }

    public void Home()
    {
        // load scene home page
        SceneManager.LoadScene("HomePage");
    }

    public void OnButtonLoginRoom()
    {
        if(isLoginRoom)
        {
            isLoginRoom = false;
            LogoutRoom();
        }
        else
        {
            isLoginRoom = true;
            LoginRoom();
        }
    }

    public void OnButtonAddCdnUrl()
    {
        string url = GameObject.Find("InputField_PublishCdnUrl1").GetComponent<InputField>().text;
        if(isAddCdnUrl)
        {
            isAddCdnUrl = false;
            engine.RemovePublishCdnUrl(publishStreamID, url, (int errorCode)=>{
                ZegoUtilHelper.PrintLogToView(string.Format("RemovePublishCdnUrl, errorCode:{0}", errorCode));
            });

            GameObject.Find("Button_AddCDNUrl").GetComponent<Button>().GetComponentInChildren<Text>().text = "Add CDN url";
        }
        else
        {
            isAddCdnUrl = true;
            engine.AddPublishCdnUrl(publishStreamID, url, (int errorCode)=>{
                ZegoUtilHelper.PrintLogToView(string.Format("AddPublishCdnUrl, errorCode:{0}", errorCode));
            });

            GameObject.Find("Button_AddCDNUrl").GetComponent<Button>().GetComponentInChildren<Text>().text = "Remove CDN url";
        } 
    }
}
