using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using ZEGO;
public class QuickStartDemo : MonoBehaviour
{
    ZegoExpressEngine engine;
    System.Random random = new System.Random();
    Text info;
    Dictionary<string, string> infos = new Dictionary<string, string>();
    string sdkVersion = "SDK_VERSION";
    string roomInfos = "ROOM_INFOS";
    string mainPublishInfos = "MAIN_PUBLISH_INFOS";
    string playInfos = "PLAY_INFOS";
    string testUser;
    string currentPlayStreamId;
    string mainPublishStreamId;
    bool mainPublishFlag = true;
    private ArrayList permissionList = new ArrayList();
    GameObject remoteVideoPlane;
    private DeviceOrientation preOrientation = DeviceOrientation.Unknown;
    // Start is called before the first frame update
    void Start()
    {
#if (UNITY_2018_3_OR_NEWER)
        permissionList.Add(Permission.Microphone);
        permissionList.Add(Permission.Camera);
#endif
        info = GameObject.Find("InformationText").GetComponent<Text>();
        if (infos.ContainsKey(sdkVersion))
        {
            infos[sdkVersion] = "SDK Version:" + ZegoExpressEngine.GetVersion() + "\n";
        }
        else
        {
            infos.Add(sdkVersion, "SDK Version:" + ZegoExpressEngine.GetVersion() + "\n");
        }
        NotifyTextDisplay();
    }

    private void NotifyTextDisplay()
    {
        string result = "";
        foreach (KeyValuePair<string, string> info in infos)
        {
            result += info.Value;
        }
        if (info != null)
        {
            info.text = result;
        }
    }

    // Update is called once per frame
    void Update()
    {
#if (UNITY_2018_3_OR_NEWER)
        CheckPermission();
#endif
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

    private void CheckPermission()
    {
#if (UNITY_2018_3_OR_NEWER)
        foreach (string permission in permissionList)
        {
            if (Permission.HasUserAuthorizedPermission(permission))
            {
            }
            else
            {
                Permission.RequestUserPermission(permission);
            }
        }
#endif
    }

    public void CreateEngine()
    {
        engine = ZegoExpressEngine.CreateEngine(123, "xxx", true, ZegoScenario.General);
    }
    public void DestroyEngine()
    {

        ZegoExpressEngine.DestroyEngine(() => { Debug.Log("destroy engine callback success"); });
    }
    public void OnExpressEngineDestroy()
    {
        Debug.Log("destroy engine callback success");
    }
    public void LoginRoom()
    {
        engine.onRoomStateUpdate = OnRoomStateUpdate;
        engine.onRoomUserUpdate = OnRoomUserUpdate;
        engine.onRoomStreamUpdate = OnRoomStreamUpdate;
        testUser = random.Next(0, 10000) + "123";
        ZegoUser user = new ZegoUser(testUser);
        engine.LoginRoom("123666", user);
    }
    public void LogOutRoom()
    {
        engine.LogoutRoom("123666");
    }
    private void OnRoomStreamUpdate(string roomId, ZegoUpdateType updateType, List<ZegoStream> streamInfoList, uint streamInfoCount)
    {
        Debug.Log("OnRoomStreamUpdate:" + "room_id:" + roomId + "\n zego_update_type:" + updateType + "\n zego_stream count:" + streamInfoList.Count + "\n =:" + streamInfoCount);
        for (int i = 0; i < streamInfoList.Count; i++)
        {
            Debug.Log("OnRoomStreamUpdate:" + "user_id:" + streamInfoList[i].user.userId + "\n user_name:" + streamInfoList[i].user.userName + "\n stream_id:" + streamInfoList[i].streamId);

        }
    }

    private void OnRoomUserUpdate(string roomId, ZegoUpdateType updateType, List<ZegoUser> userList, uint userCount)
    {
        Debug.Log("OnRoomUserUpdate:" + "room_id:" + roomId + "\n zego_update_type:" + updateType + "\n zego_user count:" + userList.Count + "\n =:" + userCount);
        for (int i = 0; i < userList.Count; i++)
        {
            Debug.Log("OnRoomUserUpdate:" + "user_id:" + userList[i].userId + "\n user_name:" + userList[i].userName);

        }
    }

    public void OnRoomStateUpdate(string roomId, ZegoRoomState state, int errorCode, string extendedData)
    {
        Debug.Log("OnRoomStateUpdate:" + "room_id:" + roomId + "\n state:" + state + "\n error_code:" + errorCode + "\n extended_data:" + extendedData);
        if (state == ZegoRoomState.Connected)
        {
            if (!infos.ContainsKey(roomInfos))
            {
                infos.Add(roomInfos, "user id:" + testUser + "  " + "user_name:" + testUser + "   " + "room_id:" + "123666" + "\n");
            }
            else
            {
                infos[roomInfos] = "user id:" + testUser + "  " + "user_name:" + testUser + "   " + "room_id:" + "123666" + "\n";
            }
            NotifyTextDisplay();
        }
        else if (state == ZegoRoomState.Disconnected)
        {
            infos.Remove(roomInfos);
            NotifyTextDisplay();
        }
    }
    public void OnPublisherStateUpdate(string streamId, ZegoPublisherState state, int errorCode, string extendedData)
    {
        Debug.Log("OnPublisherStateUpdate:" + "stream_id:" + streamId + "\n state:" + state + "\n error_code:" + errorCode + "\n extended_data:" + extendedData);
        if (streamId == mainPublishStreamId)
        {
            if (state == ZegoPublisherState.Publishing)
            {
                mainPublishFlag = false;
                if (!infos.ContainsKey(mainPublishInfos))
                {
                    infos.Add(mainPublishInfos, "main publish stream id: " + streamId + "\n");
                }
                else
                {
                    infos[mainPublishInfos] = "main publish stream id: " + streamId + "\n";
                }
                NotifyTextDisplay();
            }
            else if (state == ZegoPublisherState.NoPublish)
            {
                mainPublishFlag = true;
                infos.Remove(mainPublishInfos);
                NotifyTextDisplay();
            }
        }
    }
    public void StartPublishingStream()
    {
        if (mainPublishFlag)
        {
            mainPublishStreamId = random.Next(0, 10000) + "666";
            engine.onPublisherStateUpdate = OnPublisherStateUpdate;
            engine.onPublisherQualityUpdate = OnPublisherQualityUpdate;
            engine.StartPublishingStream(mainPublishStreamId);
        }
    }

    private void OnPublisherQualityUpdate(string streamId, ZegoPublishStreamQuality quality)
    {
        Debug.Log("OnPublisherQualityUpdate:" + "stream_id:" + streamId + "\n zego_stream_quality:" + quality.quality + "\n audio_send_bytes:" + quality.audioSendBytes + "\n video_capture_fps:" + quality.videoCaptureFps);
    }

    public void StopPublishingStream()
    {
        engine.StopPublishingStream();
    }

    RawImageVideoSurface localVideoSurface = null;
    GameObject mainLocalVideoPlane = null;
    public void OnPreviewButtonClicked()//preview use rawimage
    {
        

        mainLocalVideoPlane = GameObject.Find("MainPreViewRawImage");

        if (mainLocalVideoPlane != null && localVideoSurface == null)
        {
            localVideoSurface = mainLocalVideoPlane.AddComponent<RawImageVideoSurface>();

            localVideoSurface.SetCaptureVideoInfo();
            localVideoSurface.SetVideoSource(engine);

            localVideoSurface.transform.Rotate(180.0f, 0.0f, 0.0f);
        }


        engine.StartPreview();
    }

    public void StopPreview()
    {
        engine.StopPreview();
    }

    RendererVideoSurface remoteVideoSurface = null;
    public void OnPlayingStreamButtonClicked()
    {//pull stream use render

        GameObject obj = GameObject.Find("PlaySteamInputField");
        InputField input = (InputField)obj.GetComponent<InputField>();
        Text text = input.transform.Find("Text").GetComponent<Text>();
        if (text.text != currentPlayStreamId)
        {

            ShowPlaySteamId(text.text);
            currentPlayStreamId = text.text;
            remoteVideoPlane = GameObject.Find("PlayRender");
            if (remoteVideoPlane != null)
            {
                if (remoteVideoSurface == null)//Avoid repeated Add Component causing strange problems such as video freeze
                {
                    remoteVideoSurface = remoteVideoPlane.AddComponent<RendererVideoSurface>();
                    remoteVideoSurface.transform.Rotate(-90.0f, 0.0f, 0.0f); //Avoid repeatedly setting rotation etc.
                    remoteVideoSurface.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
                    remoteVideoSurface.transform.localScale = new Vector3(0.36f, 1f, 0.64f);
                }
                if (remoteVideoSurface != null)
                {
                    remoteVideoSurface.SetPlayVideoInfo(text.text);//Set the pull stream ID you want to display to the current control
                    remoteVideoSurface.SetVideoSource(engine);
                }
            }
            engine.onPlayerStateUpdate = OnPlayerStateUpdate;
            engine.onPlayerMediaEvent = OnPlayerMediaEvent;
            engine.onPlayerQualityUpdate = OnPlayerQualityUpdate;
            engine.StartPlayingStream(text.text);
        }
    }
    public void ShowPlaySteamId(string streamId)
    {

        if (!infos.ContainsKey(playInfos))
        {
            infos.Add(playInfos, "play stream id:" + streamId + "\n");
        }
        else
        {
            infos[playInfos] = "play stream id:" + streamId + "\n";
        }
        NotifyTextDisplay();

    }
    private void OnPlayerQualityUpdate(string streamId, ZegoPlayStreamQuality quality)
    {
        Debug.Log("OnPlayerQualityUpdate:" + "stream_id:" + streamId + "\n zego_stream_quality:" + quality.quality + "\n audio_render_fps:" + quality.audioRenderFps + "\n video_recv_fps:" + quality.videoRecvFps);

    }

    private void OnPlayerMediaEvent(string streamId, ZegoPlayerMediaEvent media_event)
    {
        Debug.Log("OnPlayerMediaEvent:" + "stream_id:" + streamId + "\n zego_player_media_event:" + media_event);

    }

    public void StopPlaySteam()
    {
        GameObject obj = GameObject.Find("PlaySteamInputField");
        InputField input = (InputField)obj.GetComponent<InputField>();
        Text text = input.transform.Find("Text").GetComponent<Text>();
        engine.StopPlayingStream(text.text);
        DismissPlayerStreamId();

    }

    private void DismissPlayerStreamId()
    {
        currentPlayStreamId = null;
        infos.Remove(playInfos);
        NotifyTextDisplay();
    }

    public void OnPlayerStateUpdate(string streamId, ZegoPlayerState state, int errorCode, string extendedData)
    {
        Debug.Log("OnPlayerStateUpdate:" + "stream_id:" + streamId + "\n state:" + state + "\n error_code:" + errorCode + "\n extended_data:" + extendedData);

    }
}
