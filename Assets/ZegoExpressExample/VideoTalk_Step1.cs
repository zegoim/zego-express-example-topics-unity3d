using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZEGO;

public class VideoTalk_Step1 : MonoBehaviour
{
    private DeviceOrientation preOrientation = DeviceOrientation.Unknown;
    InputField inputRoomID;
    InputField inputUserID;
    InputField inputPublishStreamID;
    VideoTalk notDestroyObj;
    bool isInit = false;
    // Start is called before the first frame update
    void Start()
    {
        if(ZegoExpressExample.isVideoTalkLoad == false)
        {
            ZegoExpressExample.isVideoTalkLoad = true;
            DontDestroyOnLoad(GameObject.Find("DoNotDestroyOnLoad"));
        }

        notDestroyObj = GameObject.Find("DoNotDestroyOnLoad").GetComponent<VideoTalk>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isInit == false && notDestroyObj.isActiveAndEnabled)
        {
            isInit = true;
            // get engine and init ui
            InitAll();
        }
    }

    private void OnDestroy()
    {
        
    }

    public void Home()
    {
        // reset all states
        notDestroyObj.ResetState();
        // load scene home page
        SceneManager.LoadScene("HomePage");
    }

    void LoadSceneStep2()
    {
        // load scene video talk room
        SceneManager.LoadScene("VideoTalk_Step2");
    }

    public void InitAll()
    {
        inputRoomID = GameObject.Find("InputField_RoomID").GetComponent<InputField>();
        inputRoomID.text = notDestroyObj.roomID;
        inputUserID = GameObject.Find("InputField_UserID").GetComponent<InputField>();
        inputUserID.text = notDestroyObj.user.userID;
        inputPublishStreamID = GameObject.Find("InputField_StreamID").GetComponent<InputField>();
        inputPublishStreamID.text = notDestroyObj.publishStreamID;
    }

    public void LoginRoom()
    {
        notDestroyObj.user.userID = inputUserID.text;
        notDestroyObj.user.userName = notDestroyObj.user.userID;
        notDestroyObj.publishStreamID = inputPublishStreamID.text;
        notDestroyObj.roomID = inputRoomID.text;

        LoadSceneStep2();
    }
}
