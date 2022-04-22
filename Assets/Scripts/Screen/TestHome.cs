using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System;
#if(UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif
using System.Collections;

/// <summary>
///    TestHome serves a game controller object for this application.
/// </summary>
public class TestHome : MonoBehaviour
{

    // Use this for initialization
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList();
#endif

    private InputField mInputBox;
    [SerializeField] InputField mChannelName;
    [SerializeField] InputField mUserID;
    [SerializeField] InputField mUserName;
    void Awake()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
		permissionList.Add(Permission.Microphone);         
		permissionList.Add(Permission.Camera);               
#endif
        Screen.autorotateToPortrait = false;

    }

    void Start()
    {
        mChannelName.text = AgoraUtils.GetLocalValue(AgoraConst.CHANNEL_NAME);
        mUserID.text = AgoraUtils.GetLocalValue(AgoraConst.USER_ID);
        mUserName.text = AgoraUtils.GetLocalValue(AgoraConst.RTM_USER_NAME);
    }

    void Update()
    {
        CheckPermissions();
    }

    /// <summary>
    ///   Checks for platform dependent permissions.
    /// </summary>
    private void CheckPermissions()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach(string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {                 
				Permission.RequestUserPermission(permission);
			}
        }
#endif
    }

    public void onJoinButtonClicked()
    {
        AgoraUtils.SaveLocalValue(AgoraConst.USER_ID, mUserID.text);
        AgoraUtils.SaveLocalValue(AgoraConst.CHANNEL_NAME, mChannelName.text);
        AgoraUtils.SaveLocalValue(AgoraConst.RTM_USER_NAME, mUserName.text);
        // create app if nonexistent
        SceneManager.sceneLoaded += OnLevelFinishedLoading; // configure GameObject after scene is loaded
        SceneManager.LoadScene(AgoraConst.SCREEN_PLAYGROUND, LoadSceneMode.Single);
    }

    
    public void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == AgoraConst.SCREEN_PLAYGROUND)
        {
            RtmChatManager.Instance().Login();
            DemoGameManager.Instance().Init();
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
    }
}
