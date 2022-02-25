using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZEGO;

public class PublishingStream : MonoBehaviour
{
    ZegoExpressEngine engine;
    ZegoUser user = new ZegoUser();
    string roomID;
    string publishStreamID;
    RawImageVideoSurface localVideoSurface = null;
    private DeviceOrientation preOrientation = DeviceOrientation.Unknown;
    ArrayList cameraList = new ArrayList();
    ArrayList microphoneList = new ArrayList();
    Dropdown mirrorModeDropDown;
    Text text_RoomState;
    Text text_RoomID;
    bool isLoginRoom = false;
    bool isPublish = false;
    // Start is called before the first frame update
    void Start()
    {
        CreateEngine();

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

    void OnDestroy()
    {
        Debug.Log("DestroyEngine");
        ZegoExpressEngine.DestroyEngine();
    }

    void InitAll()
    {
        user.userID = ZegoUtilHelper.UserName();
        user.userName = user.userID;

        roomID = "0002";
        publishStreamID = "0002";

        GameObject previewObj = GameObject.Find("RawImage_Preview");
        if(previewObj != null)
        {
            localVideoSurface = previewObj.AddComponent<RawImageVideoSurface>();
            localVideoSurface.SetCaptureVideoInfo();
            localVideoSurface.SetVideoSource(engine);
        }

        GameObject.Find("InputField_RoomID").GetComponent<InputField>().text = roomID;
        GameObject.Find("InputField_UserID").GetComponent<InputField>().text = user.userID;
        GameObject.Find("InputField_PublishStreamID").GetComponent<InputField>().text = publishStreamID;
        text_RoomState = GameObject.Find("Text_RoomState").GetComponent<Text>();
        text_RoomID = GameObject.Find("Text_RoomID").GetComponent<Text>();

        List<string> zegoMirrorModeList = new List<string>();
        zegoMirrorModeList.Add(ZegoVideoMirrorMode.OnlyPreviewMirror.ToString());
        zegoMirrorModeList.Add(ZegoVideoMirrorMode.BothMirror.ToString());
        zegoMirrorModeList.Add(ZegoVideoMirrorMode.NoMirror.ToString());
        zegoMirrorModeList.Add(ZegoVideoMirrorMode.OnlyPublishMirror.ToString());
        List<string> zegoViewModeList = new List<string>();
        zegoViewModeList.Add(ZegoViewMode.AspectFit.ToString());
        zegoViewModeList.Add(ZegoViewMode.AspectFill.ToString());
        zegoViewModeList.Add(ZegoViewMode.ScaleToFill.ToString());

        mirrorModeDropDown = GameObject.Find("Dropdown_MirrorMode").GetComponent<Dropdown>();
        mirrorModeDropDown.AddOptions(zegoMirrorModeList);
    }

    void BindEventHandler()
    {
        engine.onRoomStateUpdate = OnRoomStateUpdate;
        engine.onRoomUserUpdate = OnRoomUserUpdate;
        engine.onPublisherStateUpdate = OnPublisherStateUpdate;
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

            var cameraDropDown = GameObject.Find("Dropdown_Camera").GetComponent<Dropdown>();
            var microphoneDropDown = GameObject.Find("Dropdown_Microphone").GetComponent<Dropdown>();
            List<string> cameraNameList = new List<string>();
            List<string> microphoneNameList = new List<string>();
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            var cameraList = engine.GetVideoDeviceList();
            foreach(var camera in cameraList)
            {
                this.cameraList.Add(camera);
                cameraNameList.Add(camera.deviceName);
            }
            var microphoneList = engine.GetAudioDeviceList(ZegoAudioDeviceType.Input);
            foreach(var micro in microphoneList)
            {
                this.microphoneList.Add(micro);
                microphoneNameList.Add(micro.deviceName);
            }  
#else
            cameraNameList.Add("Default");
            microphoneList.Add("Default");
#endif
            cameraDropDown.AddOptions(cameraNameList);
            microphoneDropDown.AddOptions(microphoneNameList);
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
        roomID = GameObject.Find("InputField_RoomID").GetComponent<InputField>().text;
        user.userID = GameObject.Find("InputField_UserID").GetComponent<InputField>().text;
        user.userName = user.userID;
        ZegoUtilHelper.PrintLogToView(string.Format("LoginRoom, roomID:{0}, userID:{1}, userName:{2}", roomID, user.userID, user.userName));
        engine.LoginRoom(roomID, user);

        GameObject.Find("Button_LoginRoom").GetComponent<Button>().GetComponentInChildren<Text>().text = "Logout Room";
    }

    void LogoutRoom()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("LogoutRoom"));
        engine.LogoutRoom();

        GameObject.Find("Button_LoginRoom").GetComponent<Button>().GetComponentInChildren<Text>().text = "Login Room";
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
        publishStreamID = GameObject.Find("InputField_PublishStreamID").GetComponent<InputField>().text;
        GameObject.Find("Text_PreviewStreamID").GetComponent<Text>().text = publishStreamID;
        ZegoUtilHelper.PrintLogToView(string.Format("StartPublishingStream, streamID:{0}", publishStreamID));
        engine.StartPublishingStream(publishStreamID);

        GameObject.Find("Button_StartPublishing").GetComponent<Button>().GetComponentInChildren<Text>().text = "Stop Publishing";
    }

    void StopPublishingStream()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("StopPublishingStream"));
        engine.StopPublishingStream();

        GameObject.Find("Button_StartPublishing").GetComponent<Button>().GetComponentInChildren<Text>().text = "Start Publishing";
    }

    public void OnButtonStartPublishing()
    {
        if(isPublish)
        {
            isPublish = false;
            StopPreview();
            StopPublishingStream();
        }
        else
        {
            isPublish = true;
            StartPreview();
            StartPublishingStream();
        }
    }

    public void OnMirrorModeChange()
    {
        Dropdown mirrorModeDropDown = GameObject.Find("Dropdown_MirrorMode").GetComponent<Dropdown>();
        ZegoUtilHelper.PrintLogToView(string.Format("SetVideoMirrorMode, mode:{0}", mirrorModeDropDown.value));

        engine.SetVideoMirrorMode((ZegoVideoMirrorMode)mirrorModeDropDown.value);
    }

    public void OnCameraToggle()
    {
        bool isEnable = GameObject.Find("Toggle_Camera").GetComponent<Toggle>().isOn;
        ZegoUtilHelper.PrintLogToView(string.Format("EnableCamera, enable:{0}", isEnable));
        engine.EnableCamera(isEnable);
    }

    public void OnMicrophoneToggle()
    {
        bool isEnable = GameObject.Find("Toggle_Microphone").GetComponent<Toggle>().isOn;
        ZegoUtilHelper.PrintLogToView(string.Format("MuteMicrophone, mute:{0}", !isEnable));
        engine.MuteMicrophone(!isEnable);
    }

    public void OnCameraSelectChange()
    {
        int selectIndex = GameObject.Find("Dropdown_Camera").GetComponent<Dropdown>().value;
        ZegoDeviceInfo selectCamera = (ZegoDeviceInfo)cameraList[selectIndex];
        ZegoUtilHelper.PrintLogToView(string.Format("UseVideoDevice, deviceID:{0}", selectCamera.deviceID));
        engine.UseVideoDevice(selectCamera.deviceID);
    }

    public void OnMicrophoneSelectChange()
    {
        int selectIndex = GameObject.Find("Dropdown_Microphone").GetComponent<Dropdown>().value;
        ZegoDeviceInfo selectMicrophone = (ZegoDeviceInfo)microphoneList[selectIndex];
        ZegoUtilHelper.PrintLogToView(string.Format("UseVideoDevice, deviceType: Input, deviceID:{0}", selectMicrophone.deviceID));
        engine.UseAudioDevice(ZegoAudioDeviceType.Input, selectMicrophone.deviceID);
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
}
