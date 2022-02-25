using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZEGO;

public class RangeAudio : MonoBehaviour
{
    ZegoExpressEngine engine;
    ZegoRangeAudio zegoRangeAudio;
    ZegoUser user = new ZegoUser();
    string roomID;
    private ZegoUser remoteUser;
    private DeviceOrientation preOrientation = DeviceOrientation.Unknown;
    bool isLoginRoom = false;
    bool isRangeAudioCreate = false;
    float timeInterval = 1.0f;//1 second timer
    bool isUpdateSelfPosition = false;

    class ZegoPosition{
        public float[] pos = new float[3];
    }

    ConcurrentDictionary<string, ZegoPosition> userInfo = new ConcurrentDictionary<string, ZegoPosition>();

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

        timeInterval -= Time.deltaTime;

        if(timeInterval <=0)
        {
            timeInterval = 1.0f;

            // do something
            UpdateSelfPosition();
        }
    }

    void OnDestroy()
    {
        if(zegoRangeAudio != null)
        {
            engine.DestroyRangeAudio(zegoRangeAudio);
        }
        if(engine != null)
        {
            Debug.Log("DestroyEngine");
            ZegoExpressEngine.DestroyEngine();
        }
    }

    void InitAll()
    {
        user.userID = ZegoUtilHelper.UserName();
        user.userName = user.userID;

        roomID = "RangeAudio";

        GameObject.Find("InputField_RoomID").GetComponent<InputField>().text = roomID;
        GameObject.Find("InputField_UserID").GetComponent<InputField>().text = user.userID;
    }

    void BindEventHandler()
    {
        engine.onRoomStateUpdate = OnRoomStateUpdate;
        engine.onRoomUserUpdate = OnRoomUserUpdate;
        engine.onLocalDeviceExceptionOccurred = OnLocalDeviceExceptionOccurred;
        engine.onIMRecvBroadcastMessage = OnIMRecvBroadcastMessage;
        engine.onDebugError = OnDebugError;
    }

    void OnRoomStateUpdate(string roomID, ZegoRoomState state, int errorCode, string extendedData)
    {
        ZegoUtilHelper.PrintLogToView(string.Format("OnRoomStateUpdate, roomID:{0}, state:{1}, errorCode:{2}, extendedData:{3}", roomID, state, errorCode, extendedData));
        if(state == ZegoRoomState.Connected)
        {

        }
        else if(state == ZegoRoomState.Disconnected)
        {

        }
    }

    void OnRoomUserUpdate(string roomID, ZegoUpdateType updateType, List<ZegoUser> userList, uint userCount)
    {
        if(updateType == ZegoUpdateType.Add)
        {
            userList.ForEach((user)=>{
                ZegoUtilHelper.PrintLogToView(string.Format("user {0} enter room {1}", user.userID, roomID));

                ZegoPosition pos = new ZegoPosition();
                userInfo.AddOrUpdate(user.userID, pos, (oldUser, oldPos)=>pos);
            });

            // tell self position to new user in room
            isUpdateSelfPosition = true; 
        }
        else
        {
            userList.ForEach((user)=>{
                ZegoUtilHelper.PrintLogToView(string.Format("user {0} exit room {1}", user.userID, roomID));

                userInfo.TryRemove(user.userID, out ZegoPosition pos);
            });
        }

        // Update user list in room
        UpdateUserList();
    }

    void OnLocalDeviceExceptionOccurred(ZegoDeviceExceptionType exceptionType, ZegoDeviceType deviceType, string deviceID)
    {
        ZegoUtilHelper.PrintLogToView(string.Format("OnLocalDeviceExceptionOccurred, exceptionType:{0}, deviceType:{1}, deviceID:{2}", exceptionType, deviceType, deviceID));
    }

    void OnDebugError(int errorCode, string funcName, string info)
    {
        ZegoUtilHelper.PrintLogToView(string.Format("OnDebugError, funcName:{0}, info:{1}", errorCode, funcName, info));
    }

    void OnIMRecvBroadcastMessage(string roomID, List<ZegoBroadcastMessageInfo> messageList)
    {
        messageList.ForEach((message)=>{
            foreach(var user in userInfo)
            {
                if (user.Key == message.fromUser.userID)
                {
                    var posList = message.message.Split(',');
                    userInfo[message.fromUser.userID].pos[0] = float.Parse(posList[0]);
                    userInfo[message.fromUser.userID].pos[1] = float.Parse(posList[1]);
                    userInfo[message.fromUser.userID].pos[2] = float.Parse(posList[2]);

                    zegoRangeAudio.UpdateAudioSource(message.fromUser.userID, userInfo[message.fromUser.userID].pos);
                }
            }
        });

        // Recv other user's position updating message,update user list view
        UpdateUserList();
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
        roomID = GameObject.Find("InputField_RoomID").GetComponent<InputField>().text;
        user.userID = GameObject.Find("InputField_UserID").GetComponent<InputField>().text;
        user.userName = user.userID;
        
        ZegoUtilHelper.PrintLogToView(string.Format("LoginRoom, roomID:{0}, userID:{1}, userName:{2}", roomID, user.userID, user.userName));
        ZegoRoomConfig config = new ZegoRoomConfig();
        config.isUserStatusNotify = true;
        engine.LoginRoom(roomID, user, config);

        GameObject.Find("Button_LoginRoom").GetComponent<Button>().GetComponentInChildren<Text>().text = "Logout Room";
    }

    void LogoutRoom()
    {
        ZegoUtilHelper.PrintLogToView(string.Format("LogoutRoom"));
        engine.LogoutRoom();

        GameObject.Find("Button_LoginRoom").GetComponent<Button>().GetComponentInChildren<Text>().text = "Login Room";
    }

    public void CreateRangeAudio()
    {
        ZegoUtilHelper.PrintLogToView("CreateRangeAudio");
        zegoRangeAudio = engine.CreateRangeAudio();

        GameObject.Find("Button_CreateRangeAudio").GetComponent<Button>().GetComponentInChildren<Text>().text = "Destroy Range Audio";
    }

    public void DestroyRangeAudio()
    {
        ZegoUtilHelper.PrintLogToView("DestroyRangeAudio");
        engine.DestroyRangeAudio(zegoRangeAudio);
        zegoRangeAudio = null;

        GameObject.Find("Button_CreateRangeAudio").GetComponent<Button>().GetComponentInChildren<Text>().text = "Create Range Audio";
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

    public void OnButtonCreateRangeAudio()
    {
        if(isRangeAudioCreate)
        {
            isRangeAudioCreate = false;
            DestroyRangeAudio();
        }
        else
        {
            isRangeAudioCreate = true;
            CreateRangeAudio();
        }
    }

    public void OnButtonSetTeamID()
    {
        string teamID = GameObject.Find("InputField_TeamID").GetComponent<InputField>().text;

        ZegoUtilHelper.PrintLogToView(string.Format("SetTeamID, teamID:{0}", teamID));
        zegoRangeAudio.SetTeamID(teamID);
    }

    void MatrixMultiply(float[,] a, float[,] b, float[,] dst)
    {
        for(int i=0;i<3;i++)
        {
            for(int j=0;j<3;j++)
            {
                dst[i,j] = 0;
                for(int k=0;k<3;k++)
                {
                    dst[i,j] += a[i,k]*b[k,j];
                }
            }
        }
    }

    void UpdateSelfPosition()
    {
        // Update self position to other users in room via room message
        if(isUpdateSelfPosition)
        {
            isUpdateSelfPosition = false;

            float[] self_pos = new float[3]{0.0f, 0.0f, 0.0f};
            self_pos[0] = GameObject.Find("Slider_PosHead").GetComponent<Slider>().value;
            self_pos[1] = GameObject.Find("Slider_PosRight").GetComponent<Slider>().value;
            self_pos[2] = GameObject.Find("Slider_PosTop").GetComponent<Slider>().value;
            string pos_msg = string.Format("{0},{1},{2}", self_pos[0], self_pos[1], self_pos[2]);

            float theta_x = (float)(GameObject.Find("Slider_DirectionHead").GetComponent<Slider>().value * Math.PI / 180);
            float theta_y = (float)(GameObject.Find("Slider_DirectionRight").GetComponent<Slider>().value * Math.PI / 180);
            float theta_z = (float)(GameObject.Find("Slider_DirectionTop").GetComponent<Slider>().value * Math.PI / 180);

            // Right hand axis rotate
            float[,] matrix_rotate_x = new float[3,3]{
                {1,0,0},
                {0,(float)Math.Cos(theta_x),-(float)Math.Sin(theta_x)},
                {0,(float)Math.Sin(theta_x),(float)Math.Cos(theta_x)}};
            float[,] matrix_rotate_y = new float[3,3]{
                {(float)Math.Cos(theta_y),0,(float)Math.Sin(theta_y)},
                {0,1,0},
                {-(float)Math.Sin(theta_y),0,(float)Math.Cos(theta_y)}};
            float[,] matrix_rotate_z = new float[3,3]{
                {(float)Math.Cos(theta_z),-(float)Math.Sin(theta_z),0},
                {(float)Math.Sin(theta_z),(float)Math.Cos(theta_z),0},
                {0,0,1}};
            float[,] matrix_rotate = new float[3,3];
            float[,] matrix_rotate_xy = new float[3,3];
            MatrixMultiply(matrix_rotate_x, matrix_rotate_y, matrix_rotate_xy);
            MatrixMultiply(matrix_rotate_xy, matrix_rotate_z, matrix_rotate);

            var axis_forward = new float[3]{matrix_rotate[0,0], matrix_rotate[1,0], matrix_rotate[2,0]};
            var axis_right = new float[3]{matrix_rotate[0,1], matrix_rotate[1,1], matrix_rotate[2,1]};
            var axis_top = new float[3]{matrix_rotate[0,2], matrix_rotate[1,2], matrix_rotate[2,2]};

            ZegoUtilHelper.PrintLogToView(string.Format("UpdateSelfPosition, self position:{0},{1},{2}, self direction forward:{3},{4},{5}, self direction right:{6},{7},{8},self direction top:{9},{10},{11}", 
            self_pos[0],self_pos[1],self_pos[2],axis_forward[0],axis_forward[1],axis_forward[2],axis_right[0],axis_right[1],axis_right[2],axis_top[0],axis_top[1],axis_top[2]));
            zegoRangeAudio.UpdateSelfPosition(self_pos, axis_forward, axis_right, axis_top);

            // Update self position through room broadcast message for testing
            // For normal case,the position should be saved in server and sync to clients
            ZegoUtilHelper.PrintLogToView(string.Format("sendBroadcastMessage,roomID:{0},message:{1}", roomID, pos_msg));
            engine.SendBroadcastMessage(roomID, pos_msg, (int errorCode, ulong messageID)=>{
                if(errorCode != 0)
                {
                    ZegoUtilHelper.PrintLogToView(string.Format("sendBroadcastMessage fail,errorCode:{0},messageID:{1}", errorCode, messageID));
                }
            });
        }
    }

    public void Home()
    {
        // load scene home page
        SceneManager.LoadScene("HomePage");
    }

    public void OnToggleAudioModeWorld()
    {
        bool isWorld = GameObject.Find("Toggle_World").GetComponent<Toggle>().isOn;
        if(isWorld)
        {
            ZegoUtilHelper.PrintLogToView(string.Format("SetRangeAudioMode, mode:World"));
            zegoRangeAudio.SetRangeAudioMode(ZegoRangeAudioMode.World);
        }
    }

    public void OnToggleAudioModeTeam()
    {
        bool isTeam = GameObject.Find("Toggle_Team").GetComponent<Toggle>().isOn;
        if(isTeam)
        {
            ZegoUtilHelper.PrintLogToView(string.Format("SetRangeAudioMode, mode:Team"));
            zegoRangeAudio.SetRangeAudioMode(ZegoRangeAudioMode.Team);
        }
    }

    public void OnToggleMicrophone()
    {
        bool isOn = GameObject.Find("Toggle_Microphone").GetComponent<Toggle>().isOn;

        ZegoUtilHelper.PrintLogToView(string.Format("EnableMicrophone, enable:{0}", isOn));
        zegoRangeAudio.EnableMicrophone(isOn);
    }

    public void OnToggleSpeaker()
    {
        bool isOn = GameObject.Find("Toggle_Speaker").GetComponent<Toggle>().isOn;

        ZegoUtilHelper.PrintLogToView(string.Format("EnableSpeaker, enable:{0}", isOn));
        zegoRangeAudio.EnableSpeaker(isOn);
    }

    public void OnChangeReceiveRange()
    {
        float range = GameObject.Find("Slider_ReceiveRange").GetComponent<Slider>().value;

        ZegoUtilHelper.PrintLogToView(string.Format("SetAudioReceiveRange, range:{0}", range));
        zegoRangeAudio.SetAudioReceiveRange(range);
    }

    public void OnToggleSpatializer()
    {
        bool isOn = GameObject.Find("Toggle_Spatializer").GetComponent<Toggle>().isOn;

        ZegoUtilHelper.PrintLogToView(string.Format("EnableSpatializer, enable:{0}", isOn));
        zegoRangeAudio.EnableSpatializer(isOn);
    }
    public void OnToggleMuteUser()
    {
        string muteUserID = GameObject.Find("InputField_MuteUserID").GetComponent<InputField>().text;
        bool isMute = GameObject.Find("Toggle_MuteUser").GetComponent<Toggle>().isOn;

        ZegoUtilHelper.PrintLogToView(string.Format("MuteUser, userID:{0}, mute:{1}", muteUserID, isMute));
        zegoRangeAudio.MuteUser(muteUserID, isMute);
    }

    public void NeedUpdateSelfPosition()
    {
        isUpdateSelfPosition = true;
    }
    
    void UpdateUserList()
    {
        var userPosObj = GameObject.Find("Content_UserPosition").GetComponent<Text>();
        userPosObj.text = "";
        foreach (var user in userInfo)
        {
            string posInfo = string.Format("userID:{0}, position:({1},{2},{3})", user.Key, user.Value.pos[0], user.Value.pos[1], user.Value.pos[2]);

            if(userPosObj.text == "")
            {
                userPosObj.text = posInfo;
            }
            else
            {
                userPosObj.text += Environment.NewLine + posInfo;
            }
        }
    }
}
