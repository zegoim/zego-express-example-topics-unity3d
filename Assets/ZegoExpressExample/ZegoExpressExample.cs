using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using ZEGO;


class ZegoExpressExample : MonoBehaviour
{
    private ArrayList permissionList = new ArrayList();
    private DeviceOrientation preOrientation = DeviceOrientation.Unknown;

    public static bool isVideoTalkLoad = false;
    // Start is called before the first frame update
    void Start()
    {
        CheckPermission();
        
        // request permissions
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnApplicationQuit()
    {//It must be added for unity editor debugging, otherwise it will cause unity to be stuck

    }

    private void CheckPermission()
    {
#if (UNITY_2018_3_OR_NEWER)
        permissionList.Add(Permission.Microphone);
        permissionList.Add(Permission.Camera);
        foreach (string permission in permissionList)
        {
            if (Permission.HasUserAuthorizedPermission(permission) == false)
            {
                Permission.RequestUserPermission(permission);
            }
        }
#endif
    }

    public void OnClickVideoTalk()
    {
        SceneManager.LoadScene("VideoTalk_Step1", LoadSceneMode.Single);
    }

    public void OnClickCommonUsage()
    {
        SceneManager.LoadScene("CommonUsage", LoadSceneMode.Single);
    }

    public void OnClickPublishingStream()
    {
        SceneManager.LoadScene("PublishingStream");
    }

    public void OnClickPlayingStream()
    {
        SceneManager.LoadScene("PlayingStream");
    }

    public void OnClickStreamMonitoring()
    {
        SceneManager.LoadScene("StreamMonitoring");
    }

    public void OnClickStreamByCDN()
    {
        SceneManager.LoadScene("StreamByCDN");
    }

    public void OnClickRangeAudio()
    {
        SceneManager.LoadScene("RangeAudio");
    }

    public void OnClickVoiceReverbStereo()
    {
        SceneManager.LoadScene("VoiceReverbStereo");
    }

    public void OnClickSetting()
    {
        SceneManager.LoadScene("Setting");
    }
}
