
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZEGO;

public class CommonUsage : MonoBehaviour
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
    Dropdown publishViewModeDropDown;
    Dropdown playViewModeDropDown;
    Text text_RoomState;
    Text text_RoomID;

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

    void OnDestroy()
    {
        Debug.Log("DestroyEngine");
        ZegoExpressEngine.DestroyEngine();
    }

    void InitAll()
    {
        user.userID = ZegoUtilHelper.UserName();
        user.userName = user.userID;

        roomID = "0001";
        publishStreamID = "0001";
        playStreamID = "0001";

        GameObject previewObj = GameObject.Find("RawImage_Preview");
        if(previewObj != null)
        {
            localVideoSurface = previewObj.AddComponent<RawImageVideoSurface>();
            localVideoSurface.SetCaptureVideoInfo();
            //localVideoSurface.SetVideoSource(engine);
        }

        GameObject remoteVideoPlane = GameObject.Find("RawImage_Play");
        if (remoteVideoPlane != null)
        {
            if (remoteVideoSurface == null)//Avoid repeated Add Component causing strange problems such as video freeze
            {
                remoteVideoSurface = remoteVideoPlane.AddComponent<RawImageVideoSurface>();
                remoteVideoSurface.SetPlayVideoInfo(playStreamID);
                //remoteVideoSurface.SetVideoSource(engine);
            }
        }

        GameObject.Find("InputField_RoomID").GetComponent<InputField>().text = roomID;
        GameObject.Find("InputField_UserID").GetComponent<InputField>().text = user.userID;
        GameObject.Find("InputField_PublishStreamID").GetComponent<InputField>().text = publishStreamID;
        GameObject.Find("InputField_PlayStreamID").GetComponent<InputField>().text = playStreamID;
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
        engine.onPlayerStateUpdate = OnPlayerStateUpdate;
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
            localVideoSurface.SetVideoSource(engine);
            remoteVideoSurface.SetVideoSource(engine);
        }
    }

    public void DestroyEngine()
    {
        if(engine != null)
        {
            ZegoUtilHelper.PrintLogToView("DestroyEngine");
            ZegoExpressEngine.DestroyEngine();
            engine = null;
        }
    }

    public void LoginRoom()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        roomID = GameObject.Find("InputField_RoomID").GetComponent<InputField>().text;
        user.userID = GameObject.Find("InputField_UserID").GetComponent<InputField>().text;
        user.userName = user.userID;
        ZegoUtilHelper.PrintLogToView(string.Format("LoginRoom, roomID:{0}, userID:{1}, userName:{2}", roomID, user.userID, user.userName));
        engine.LoginRoom(roomID, user);
    }

    public void StartPreview()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        ZegoUtilHelper.PrintLogToView("StartPreview");
        engine.StartPreview();
    }

    public void StartPublishingStream()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        publishStreamID = GameObject.Find("InputField_PublishStreamID").GetComponent<InputField>().text;
        GameObject.Find("Text_PreviewStreamID").GetComponent<Text>().text = publishStreamID;
        ZegoUtilHelper.PrintLogToView(string.Format("StartPublishingStream, streamID:{0}", publishStreamID));
        engine.StartPublishingStream(publishStreamID);
    }

    public void StartPlayingStream()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        playStreamID = GameObject.Find("InputField_PlayStreamID").GetComponent<InputField>().text;
        GameObject.Find("Text_PlayStreamID").GetComponent<Text>().text = playStreamID;
        if (remoteVideoSurface != null)
        {
            ZegoUtilHelper.PrintLogToView(string.Format("SetPlayVideoInfo, streamID:{0}", playStreamID));
            remoteVideoSurface.SetPlayVideoInfo(playStreamID);//Set the pull stream ID you want to display to the current control
        }
        
        ZegoUtilHelper.PrintLogToView(string.Format("StartPlayingStream, streamID:{0}", playStreamID));
        engine.StartPlayingStream(playStreamID);
    }

    public void OnButtonStartPublishing()
    {
        StartPreview();
        StartPublishingStream();
    }

    public void onMirrorModeChange()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        Dropdown mirrorModeDropDown = GameObject.Find("Dropdown_MirrorMode").GetComponent<Dropdown>();
        ZegoUtilHelper.PrintLogToView(string.Format("SetVideoMirrorMode, mode:{0}", mirrorModeDropDown.value));

        engine.SetVideoMirrorMode((ZegoVideoMirrorMode)mirrorModeDropDown.value);
    }

    public void onCameraToggle()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        bool isEnable = GameObject.Find("Toggle_Camera").GetComponent<Toggle>().isOn;
        ZegoUtilHelper.PrintLogToView(string.Format("EnableCamera, enable:{0}", isEnable));
        engine.EnableCamera(isEnable);
    }

    public void onMicrophoneToggle()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        bool isEnable = GameObject.Find("Toggle_Microphone").GetComponent<Toggle>().isOn;
        ZegoUtilHelper.PrintLogToView(string.Format("MuteMicrophone, mute:{0}", !isEnable));
        engine.MuteMicrophone(!isEnable);
    }

    public void onCameraSelectChange()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        int selectIndex = GameObject.Find("Dropdown_Camera").GetComponent<Dropdown>().value;
        ZegoDeviceInfo selectCamera = (ZegoDeviceInfo)cameraList[selectIndex];
        ZegoUtilHelper.PrintLogToView(string.Format("UseVideoDevice, deviceID:{0}", selectCamera.deviceID));
        engine.UseVideoDevice(selectCamera.deviceID);
    }

    public void onMicrophoneSelectChange()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        int selectIndex = GameObject.Find("Dropdown_Microphone").GetComponent<Dropdown>().value;
        ZegoDeviceInfo selectMicrophone = (ZegoDeviceInfo)microphoneList[selectIndex];
        ZegoUtilHelper.PrintLogToView(string.Format("UseVideoDevice, deviceType: Input, deviceID:{0}", selectMicrophone.deviceID));
        engine.UseAudioDevice(ZegoAudioDeviceType.Input, selectMicrophone.deviceID);
    }

    public void onVideoToggle()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        bool mute = !GameObject.Find("Toggle_Video").GetComponent<Toggle>().isOn;
        ZegoUtilHelper.PrintLogToView(string.Format("MutePlayStreamVideo, streamID: {0}, mute:{1}", playStreamID, mute));
        engine.MutePlayStreamVideo(playStreamID, mute);
    }

    public void onAudioToggle()
    {
        if(engine == null)
        {
            ZegoUtilHelper.PrintLogToView("Engine not created!");
            return;
        }
        bool mute = !GameObject.Find("Toggle_Audio").GetComponent<Toggle>().isOn;
        ZegoUtilHelper.PrintLogToView(string.Format("MutePlayStreamAudio, streamID: {0}, mute:{1}", playStreamID, mute));
        engine.MutePlayStreamAudio(playStreamID, mute);
    }

    public void Home()
    {
        // load scene home page
        SceneManager.LoadScene("HomePage");
    }
}
