using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZEGO;

public class VideoTalk_Step2 : MonoBehaviour
{
    ZegoExpressEngine engine;
    string roomID = "0001";
    public string playStreamID = "";
    string playUserID = "";
    private ArrayList roomStreamList = new ArrayList();
    private DeviceOrientation preOrientation = DeviceOrientation.Unknown;
    RawImageVideoSurface localVideoSurface = null;
    RawImageVideoSurface remoteVideoSurface = null;
    VideoTalk notDestroyObj;
    // Start is called before the first frame update
    void Start()
    {
        notDestroyObj = GameObject.Find("DoNotDestroyOnLoad").GetComponent<VideoTalk>();
        engine = notDestroyObj.GetEngine();
        InitAll();

        BindEventHandler();

        LoginRoom();

        // Start preview
        StartPreview();
        // Start publish stream
        StartPublishingStream();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void InitAll()
    {
        GameObject.Find("Text_RoomID").GetComponent<Text>().text = notDestroyObj.roomID;
        GameObject.Find("Text_RoomState").GetComponent<Text>().text = "Connected";
            
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
    }

    void BindEventHandler()
    {
        engine.onRoomStateUpdate = OnRoomStateUpdate;
        engine.onRoomStreamUpdate = OnRoomStreamUpdate;
        engine.onRoomUserUpdate = OnRoomUserUpdate;
        engine.onPublisherStateUpdate = OnPublisherStateUpdate;
        engine.onPlayerStateUpdate = OnPlayerStateUpdate;
        engine.onDebugError = OnDebugError;
    }

    void OnRoomStateUpdate(string roomID, ZegoRoomState state, int errorCode, string extendedData)
    {
        if(roomID == this.roomID)
        {
            // error, return to login page
            if(errorCode != 0)
            {
                SceneManager.LoadScene("VideoTalk_step1");
                return;
            }

            // login room, load video talk page
            if(state == ZegoRoomState.Connected)
            {
                
            }
            else if (state == ZegoRoomState.Connecting)
            {

            } else if (state == ZegoRoomState.Disconnected)
            {

            }
        }
    }

    void OnRoomStreamUpdate(string roomID, ZegoUpdateType updateType, List<ZegoStream> streamList, string extendedData)
    {
        streamList.ForEach((stream) =>
        {
            bool isFind = false;
            foreach(ZegoStream stream1 in roomStreamList)
            {
                if(stream.streamID == stream1.streamID)
                {
                    if(updateType == ZegoUpdateType.Delete)
                    {
                        roomStreamList.Remove(stream1);
                    }
                    else
                    {

                    }
                    isFind = true;
                    break;
                }
            }
            if(isFind == false)
            {
                roomStreamList.Add(stream);
            }
        });

        if(playStreamID != "")
        {
            StopPlayingStream(playStreamID);
            playStreamID = "";
        }

        if(roomStreamList.Count > 0)
        {
            playStreamID = ((ZegoStream)roomStreamList[roomStreamList.Count - 1]).streamID;
            playUserID = ((ZegoStream)roomStreamList[roomStreamList.Count - 1]).user.userID;

            GameObject.Find("Text_PlayUserID").GetComponent<Text>().text = playUserID;
            GameObject.Find("Text_PlayStreamID").GetComponent<Text>().text = playStreamID;
            StartPlayingStream(playStreamID);
        }
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

    public void LoginRoom()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("LoginRoom, roomID:{0}, userID:{1}, userName:{2}", notDestroyObj.roomID, notDestroyObj.user.userID, notDestroyObj.user.userName));
        ZegoRoomConfig config = new ZegoRoomConfig();
        config.isUserStatusNotify = true;
        engine.LoginRoom(notDestroyObj.roomID, notDestroyObj.user, config);
    }

    public void StartPreview()
    {
        ZegoUtilHelper.PrintLogToView("StartPreview");
        engine.StartPreview();
    }

    public void StopPreview()
    {
        ZegoUtilHelper.PrintLogToView("StopPreview");
        engine.StopPreview();
    }

    public void StartPublishingStream()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("StartPublishingStream, streamID:{0}", notDestroyObj.publishStreamID));
        engine.StartPublishingStream(notDestroyObj.publishStreamID);
    }

    public void StopPublishingStream()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("StopPublishingStream"));
        engine.StopPublishingStream();
    }

    public void StartPlayingStream(string streamID)
    {
        if (remoteVideoSurface != null)
        {
            remoteVideoSurface.SetPlayVideoInfo(streamID);//Set the pull stream ID you want to display to the current control
        }
        
        ZegoUtilHelper.PrintLogToView(string.Format("StartPlayingStream, streamID:{0}", streamID));
        engine.StartPlayingStream(streamID);
    }

    public void StopPlayingStream(string streamID)
    {
        ZegoUtilHelper.PrintLogToView(string.Format("StopPlayingStream, streamID:{0}", streamID));
        engine.StopPlayingStream(streamID);
    }

    void LoadSceneStep1()
    {
        // load scene video talk room
        SceneManager.LoadScene("VideoTalk_Step1");
    }

    public void OnButtonStop()
    {
        // destroy engine and return to login page
        notDestroyObj.DestroyEngine();
        LoadSceneStep1();
    }
}
