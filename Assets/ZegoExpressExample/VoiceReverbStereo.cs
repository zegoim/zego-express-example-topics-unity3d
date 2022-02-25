using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZEGO;

public class VoiceReverbStereo : MonoBehaviour
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
    Text text_PublisherQuality;
    Text text_PlayerQuality;
    bool isLoginRoom = false;
    bool isPublish = false;
    bool isPlay = false;
    int playWidth;
    int playHeight;
    ArrayList zegoReverbEchoParamPresetList = new ArrayList();
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

        roomID = "0012";
        publishStreamID = "0012";
        playStreamID = "0012";

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

        GameObject.Find("InputField_PublishStreamID").GetComponent<InputField>().text = publishStreamID;
        GameObject.Find("InputField_PlayStreamID").GetComponent<InputField>().text = playStreamID;
        text_RoomState = GameObject.Find("Text_RoomState").GetComponent<Text>();
        text_RoomID = GameObject.Find("Text_RoomID").GetComponent<Text>();

        List<string> zegoAudioCaptureStereoList = new List<string>();
        zegoAudioCaptureStereoList.Add(ZegoAudioCaptureStereoMode.None.ToString());
        zegoAudioCaptureStereoList.Add(ZegoAudioCaptureStereoMode.Always.ToString());
        zegoAudioCaptureStereoList.Add(ZegoAudioCaptureStereoMode.Adaptive.ToString());
        GameObject.Find("Dropdown_CaptureStereo").GetComponent<Dropdown>().AddOptions(zegoAudioCaptureStereoList);

        List<string> zegoVoiceChargerPresetList = new List<string>();
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.None.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.MenToChild.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.MenToWomen.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.WomenToChild.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.WomenToMen.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.Foreigner.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.OptimusPrime.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.Android.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.Ethereal.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.MaleMagnetic.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.FemaleFresh.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.MajorC.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.MinorA.ToString());
        zegoVoiceChargerPresetList.Add(ZegoVoiceChangerPreset.HarmonicMinor.ToString());
        GameObject.Find("Dropdown_VoiceChargerPreset").GetComponent<Dropdown>().AddOptions(zegoVoiceChargerPresetList);

        List<string> zegoReverbList = new List<string>();
        zegoReverbList.Add(ZegoReverbPreset.None.ToString());
        zegoReverbList.Add(ZegoReverbPreset.SoftRoom.ToString());
        zegoReverbList.Add(ZegoReverbPreset.LargeRoom.ToString());
        zegoReverbList.Add(ZegoReverbPreset.ConcertHall.ToString());
        zegoReverbList.Add(ZegoReverbPreset.Valley.ToString());
        zegoReverbList.Add(ZegoReverbPreset.RecordingStudio.ToString());
        zegoReverbList.Add(ZegoReverbPreset.Basement.ToString());
        zegoReverbList.Add(ZegoReverbPreset.KTV.ToString());
        zegoReverbList.Add(ZegoReverbPreset.Popular.ToString());
        zegoReverbList.Add(ZegoReverbPreset.Rock.ToString());
        zegoReverbList.Add(ZegoReverbPreset.VocalConcert.ToString());
        zegoReverbList.Add(ZegoReverbPreset.GramoPhone.ToString());
        GameObject.Find("Dropdown_ReverbPreset").GetComponent<Dropdown>().AddOptions(zegoReverbList);

        List<string> zegoReverbEchoPresetList = new List<string>();
        zegoReverbEchoPresetList.Add("None");
        zegoReverbEchoPresetList.Add("Ethereal");
        zegoReverbEchoPresetList.Add("Robot");
        zegoReverbEchoPresetList.Add("Custom");
        GameObject.Find("Dropdown_ReverbEchoPreset").GetComponent<Dropdown>().AddOptions(zegoReverbEchoPresetList);

        ZegoReverbEchoParam paramNone = new ZegoReverbEchoParam();
        paramNone.inGain = 1;
        paramNone.outGain = 1;
        paramNone.numDelays = 0;
        for(int i=0;i<7;i++)
        {
            paramNone.delay[i] = 0;
        }
        for(int i=0;i<7;i++)
        {
            paramNone.decay[i] = 0;
        }
        zegoReverbEchoParamPresetList.Add(paramNone);

        ZegoReverbEchoParam paramEthereal = new ZegoReverbEchoParam();
        paramEthereal.inGain = 0.8f;
        paramEthereal.outGain = 1;
        paramEthereal.numDelays = 7;
        paramEthereal.delay[0] = 230;
        paramEthereal.delay[1] = 460;
        paramEthereal.delay[2] = 690;
        paramEthereal.delay[3] = 920;
        paramEthereal.delay[4] = 1150;
        paramEthereal.delay[5] = 1380;
        paramEthereal.delay[6] = 1610;

        paramEthereal.decay[0] = 0.41f;
        paramEthereal.decay[1] = 0.18f;
        paramEthereal.decay[2] = 0.08f;
        paramEthereal.decay[3] = 0.03f;
        paramEthereal.decay[4] = 0.009f;
        paramEthereal.decay[5] = 0.003f;
        paramEthereal.decay[6] = 0.001f;

        zegoReverbEchoParamPresetList.Add(paramEthereal);

        ZegoReverbEchoParam paramRobot = new ZegoReverbEchoParam();
        paramRobot.inGain = 0.8f;
        paramRobot.outGain = 1;
        paramRobot.numDelays = 7;
        paramRobot.delay[0] = 60;
        paramRobot.delay[1] = 120;
        paramRobot.delay[2] = 180;
        paramRobot.delay[3] = 240;
        paramRobot.delay[4] = 300;
        paramRobot.delay[5] = 360;
        paramRobot.delay[6] = 420;

        paramRobot.decay[0] = 0.51f;
        paramRobot.decay[1] = 0.26f;
        paramRobot.decay[2] = 0.12f;
        paramRobot.decay[3] = 0.05f;
        paramRobot.decay[4] = 0.02f;
        paramRobot.decay[5] = 0.009f;
        paramRobot.decay[6] = 0.001f;

        zegoReverbEchoParamPresetList.Add(paramRobot);

        ZegoReverbEchoParam paramCustom = new ZegoReverbEchoParam();
        paramCustom.inGain = 0.8f;
        paramCustom.outGain = 1;
        paramCustom.numDelays = 7;
        paramCustom.delay[0] = 300;
        paramCustom.delay[1] = 600;
        paramCustom.delay[2] = 900;
        paramCustom.delay[3] = 0;
        paramCustom.delay[4] = 0;
        paramCustom.delay[5] = 0;
        paramCustom.delay[6] = 0;

        paramCustom.decay[0] = 0.3f;
        paramCustom.decay[1] = 0.2f;
        paramCustom.decay[2] = 0.1f;
        paramCustom.decay[3] = 0;
        paramCustom.decay[4] = 0;
        paramCustom.decay[5] = 0;
        paramCustom.decay[6] = 0;

        zegoReverbEchoParamPresetList.Add(paramCustom);
    }

    void BindEventHandler()
    {
        engine.onRoomStateUpdate = OnRoomStateUpdate;
        engine.onRoomUserUpdate = OnRoomUserUpdate;
        engine.onPublisherStateUpdate = OnPublisherStateUpdate;
        engine.onPlayerStateUpdate = OnPlayerStateUpdate;
        engine.onLocalDeviceExceptionOccurred = OnLocalDeviceExceptionOccurred;
        engine.onPlayerVideoSizeChanged = OnPlayerVideoSizeChanged;
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

    void OnPlayerVideoSizeChanged(string streamID, int width, int height)
    {
        playWidth = width;
        playHeight = height;
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

    void StartPlaying()
    {
        playStreamID = GameObject.Find("InputField_PlayStreamID").GetComponent<InputField>().text;
        if (remoteVideoSurface != null)
        {
            ZegoUtilHelper.PrintLogToView(string.Format("SetPlayVideoInfo, streamID:{0}", playStreamID));
            remoteVideoSurface.SetPlayVideoInfo(playStreamID);//Set the pull stream ID you want to display to the current control
        }
        
        ZegoUtilHelper.PrintLogToView(string.Format("StartPlayingStream, streamID:{0}", playStreamID));
        engine.StartPlayingStream(playStreamID);

        GameObject.Find("Button_StartPlaying").GetComponent<Button>().GetComponentInChildren<Text>().text = "Stop Playing";
    }

    void StopPlaying()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("StopPlayingStream, streamID:{0}", playStreamID));
        engine.StopPlayingStream(playStreamID);

        GameObject.Find("Button_StartPlaying").GetComponent<Button>().GetComponentInChildren<Text>().text = "Start Playing";
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

    public void OnToggleEncoderStereo()
    {
        bool isOn = GameObject.Find("Toggle_EncoderStereo").GetComponent<Toggle>().isOn;

        if(isOn)
        {
            ZegoAudioConfig config = new ZegoAudioConfig(ZegoAudioConfigPreset.HighQualityStereo);

            ZegoUtilHelper.PrintLogToView(string.Format("SetAudioConfig, preset:{0}", ZegoAudioConfigPreset.HighQualityStereo));
            engine.SetAudioConfig(config);
        }
        else
        {
            ZegoAudioConfig config = new ZegoAudioConfig(ZegoAudioConfigPreset.HighQuality);

            ZegoUtilHelper.PrintLogToView(string.Format("SetAudioConfig, preset:{0}", ZegoAudioConfigPreset.HighQuality));
            engine.SetAudioConfig(config);
        }
    }

    public void OnSelectCaptureStereo()
    {
        int selectIndex = GameObject.Find("Dropdown_CaptureStereo").GetComponent<Dropdown>().value;
        ZegoAudioCaptureStereoMode mode = (ZegoAudioCaptureStereoMode)selectIndex;

        ZegoUtilHelper.PrintLogToView(string.Format("SetAudioCaptureStereoMode, mode:{0}", mode));
        engine.SetAudioCaptureStereoMode(mode);
    }

    public void OnSelectVoiceChargerPreset()
    {
        int selectIndex = GameObject.Find("Dropdown_VoiceChargerPreset").GetComponent<Dropdown>().value;
        ZegoVoiceChangerPreset peset = (ZegoVoiceChangerPreset)selectIndex;

        ZegoUtilHelper.PrintLogToView(string.Format("SetVoiceChangerPreset, peset:{0}", peset));
        engine.SetVoiceChangerPreset(peset);
    }

    public void OnChangePitch()
    {
        ZegoVoiceChangerParam pp = new ZegoVoiceChangerParam();
        pp.pitch = GameObject.Find("Slider_Pitch").GetComponent<Slider>().value;

        ZegoUtilHelper.PrintLogToView(string.Format("SetVoiceChangerParam, pitch:{0}", pp.pitch));
        engine.SetVoiceChangerParam(pp);
    }

    public void OnToggleVoiceChangerCustomParam()
    {
        var isOn = GameObject.Find("Toggle_CustomParam").GetComponent<Toggle>().isOn;

        GameObject.Find("Slider_Pitch").GetComponent<Slider>().enabled = isOn;
    }

    public void OnSelectReverbPreset()
    {
        ZegoReverbPreset preset = (ZegoReverbPreset)GameObject.Find("Dropdown_ReverbPreset").GetComponent<Dropdown>().value;

        ZegoUtilHelper.PrintLogToView(string.Format("SetReverbPreset, preset:{0}", preset));
        engine.SetReverbPreset(preset);
    }

    public void OnToggleReverbCustomParam()
    {
        var isOn = GameObject.Find("Toggle_ReverbCustomParam").GetComponent<Toggle>().isOn;

        EnableReverbControls(isOn);
    }

    void EnableReverbControls(bool enable)
    {
        GameObject.Find("Slider_RoomSize").GetComponent<Slider>().enabled = enable;
        GameObject.Find("Slider_Reverberance").GetComponent<Slider>().enabled = enable;
        GameObject.Find("Slider_Damping").GetComponent<Slider>().enabled = enable;
        GameObject.Find("Toggle_WetOnly").GetComponent<Toggle>().enabled = enable;
        GameObject.Find("Slider_WetGain").GetComponent<Slider>().enabled = enable;
        GameObject.Find("Slider_DryGain").GetComponent<Slider>().enabled = enable;
        GameObject.Find("Slider_ToneLow").GetComponent<Slider>().enabled = enable;
        GameObject.Find("Slider_ToneHigh").GetComponent<Slider>().enabled = enable;
        GameObject.Find("Slider_PreDelay").GetComponent<Slider>().enabled = enable;
        GameObject.Find("Slider_StereoWidth").GetComponent<Slider>().enabled = enable;
    }

    public void UpdateReverbAdvancedParam()
    {
        ZegoReverbAdvancedParam pp = new ZegoReverbAdvancedParam();
        pp.damping = GameObject.Find("Slider_Damping").GetComponent<Slider>().value;
        pp.dryGain = GameObject.Find("Slider_DryGain").GetComponent<Slider>().value;
        pp.preDelay = GameObject.Find("Slider_PreDelay").GetComponent<Slider>().value;
        pp.reverberance = GameObject.Find("Slider_Reverberance").GetComponent<Slider>().value;
        pp.roomSize = GameObject.Find("Slider_RoomSize").GetComponent<Slider>().value;
        pp.stereoWidth = GameObject.Find("Slider_StereoWidth").GetComponent<Slider>().value;
        pp.toneHigh = GameObject.Find("Slider_ToneHigh").GetComponent<Slider>().value;
        pp.toneLow = GameObject.Find("Slider_ToneLow").GetComponent<Slider>().value;
        pp.wetGain = GameObject.Find("Slider_WetGain").GetComponent<Slider>().value;
        pp.wetOnly = GameObject.Find("Toggle_WetOnly").GetComponent<Toggle>().isOn;
    }

    public void OnSelectReverbEchoPreset()
    {
        int index = GameObject.Find("Dropdown_ReverbEchoPreset").GetComponent<Dropdown>().value;

        ZegoUtilHelper.PrintLogToView(string.Format("SetReverbEchoParam"));
        engine.SetReverbEchoParam((ZegoReverbEchoParam)zegoReverbEchoParamPresetList[index]);
    }

    public void OnToggleVirtualStereo()
    {
        bool isOn = GameObject.Find("Toggle_VirtualStereo").GetComponent<Toggle>().isOn;
        int angle = (int)GameObject.Find("Slider_Angle").GetComponent<Slider>().value;

        ZegoUtilHelper.PrintLogToView(string.Format("EnableVirtualStereo, enable:{0}, angle:{1}", isOn, angle));
        engine.EnableVirtualStereo(isOn, angle);
    }

    public void OnSliderAngle()
    {
        bool isOn = GameObject.Find("Toggle_VirtualStereo").GetComponent<Toggle>().isOn;

        if(isOn)
        {
            int angle = (int)GameObject.Find("Slider_Angle").GetComponent<Slider>().value;
            ZegoUtilHelper.PrintLogToView(string.Format("EnableVirtualStereo, enable:{0}, angle:{1}", isOn, angle));
            engine.EnableVirtualStereo(isOn, angle);
        }
    }
}
