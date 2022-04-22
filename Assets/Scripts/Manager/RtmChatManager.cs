using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using agora_rtm;

public class RtmChatManager :MonoSingleton<RtmChatManager> 
{

    private RtmClient rtmClient = null;
    private RtmChannel channel;
    private RtmCallManager callManager;

    private RtmClientEventHandler clientEventHandler;
    private RtmChannelEventHandler channelEventHandler;
    private RtmCallEventHandler callEventHandler;

    string _userName = "";
    string UserName {
        get { return _userName; }
        set {
            _userName = value;
            PlayerPrefs.SetString("RTM_USER", _userName);
            PlayerPrefs.Save();
        }
    }

    string _channelName = "";
    string ChannelName
    {
        get { return _channelName; }
        set {
            _channelName = value;
            PlayerPrefs.SetString("RTM_CHANNEL", _channelName);
            PlayerPrefs.Save();
        }
    }

    agora_rtm.SendMessageOptions _MessageOptions = new agora_rtm.SendMessageOptions() {
                enableOfflineMessaging = true,
                enableHistoricalMessaging = true
	};

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        clientEventHandler = new RtmClientEventHandler();
        channelEventHandler = new RtmChannelEventHandler();
        callEventHandler = new RtmCallEventHandler();

        rtmClient = new RtmClient(AgoraConst.APP_ID_RTM, clientEventHandler);
#if UNITY_EDITOR
        rtmClient.SetLogFile("./rtm_log.txt");
#endif

        clientEventHandler.OnQueryPeersOnlineStatusResult = OnQueryPeersOnlineStatusResultHandler;
        clientEventHandler.OnLoginSuccess = OnClientLoginSuccessHandler;
        clientEventHandler.OnLoginFailure = OnClientLoginFailureHandler;
        clientEventHandler.OnMessageReceivedFromPeer = OnMessageReceivedFromPeerHandler;

        channelEventHandler.OnJoinSuccess = OnJoinSuccessHandler;
        channelEventHandler.OnJoinFailure = OnJoinFailureHandler;
        channelEventHandler.OnLeave = OnLeaveHandler;
        channelEventHandler.OnMessageReceived = OnChannelMessageReceivedHandler;

        // Optional, tracking members
        channelEventHandler.OnGetMembers = OnGetMembersHandler;
        channelEventHandler.OnMemberCountUpdated = OnMemberCountUpdatedHandler;
        channelEventHandler.OnMemberJoined = OnMemberJoinedHandler;
        channelEventHandler.OnMemberLeft = OnMemberLeftHandler;

        // image
        clientEventHandler.OnSendMessageResult = OnSendMessageResultHandler;
        clientEventHandler.OnMediaDownloadToFileResult = OnMediaDownloadToFileResultHandler;
        clientEventHandler.OnMediaDownloadToMemoryResult = OnMediaDownloadToMemoryResultHandler;

        // invite
        callEventHandler.OnLocalInvitationAccepted = OnLocalInvitationAcceptedHandler;
        callEventHandler.OnLocalInvitationCanceled = OnLocalInvitationCanceledHandler;
        callEventHandler.OnLocalInvitationFailure = OnLocalInvitationFailureHandler;
        callEventHandler.OnLocalInvitationReceivedByPeer = OnLocalInvitationReceivedByPeerHandler;
        callEventHandler.OnLocalInvitationRefused = OnLocalInvitationRefusedHandler;
            
	    callEventHandler.OnRemoteInvitationAccepted = OnRemoteInvitationAcceptedHandler;
        callEventHandler.OnRemoteInvitationCanceled = OnRemoteInvitationCanceledHandler;
        callEventHandler.OnRemoteInvitationFailure = OnRemoteInvitationFailureHandler;
        callEventHandler.OnRemoteInvitationReceived = OnRemoteInvitationReceivedHandler;
        callEventHandler.OnRemoteInvitationRefused = OnRemoteInvitationRefusedHandler;

        callManager = rtmClient.GetRtmCallManager(callEventHandler);
        // state
        clientEventHandler.OnConnectionStateChanged = OnConnectionStateChangedHandler;

        bool initialized = ShowDisplayTexts();
        if (initialized)
        {
            string ver = RtmClient.GetSdkVersion();
            Debug.Log("RTM:: engine version " + ver + " initialized.");
        }
        else
        {
            Debug.Log("RTM:: engine not initialized");
        }
    }

    void OnApplicationQuit()
    {
        if (channel != null)
        {
            channel.Dispose();
            channel = null;
        }
        if (rtmClient != null)
        {
            rtmClient.Dispose();
            rtmClient = null;
        }
    }
    #region Button Events
    public void Login()
    {
        UserName = AgoraUtils.GetLocalValue(AgoraConst.RTM_USER_NAME);

        if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(AgoraConst.APP_ID_RTM))
        {
            Debug.LogError("RTM::We need a username and appId to login");
            return;
        }
        CommonVars.LOCAL_USER_NAME = UserName;
        rtmClient.Login(AgoraConst.TOKEN_RTM, UserName);
    }

    public void Logout()
    {
        AgoraDebug.Log(UserName + " logged out of the rtm",  AgoraDebug.Color.BLUE);
        rtmClient.Logout();
    }

    public void ChannelMemberCountButtonPressed()
    {
        if (channel != null)
        {
            channel.GetMembers();
        }
    }

    public void JoinChannel()
    {
        ChannelName = AgoraUtils.GetLocalValue(AgoraConst.CHANNEL_NAME);
        channel = rtmClient.CreateChannel(ChannelName, channelEventHandler);
        ShowCurrentChannelName();
        channel.Join();
    }

    public void LeaveChannel()
    {
        AgoraDebug.Log(UserName + " left the chat",  AgoraDebug.Color.BLUE);
        channel.Leave();
    }

    public void SendMessageToChannel(string message = "")
    {
        if(message != null && message != "")channel.SendMessage(rtmClient.CreateMessage(message));
    }


    public void QueryPeersOnlineStatus()
    {
        long req = 222222;
        //rtmClient.QueryPeersOnlineStatus(new string[] { queryUsersBox.text }, req);
    }

    #region  --Image Send / Receive ---------------------------
    /*string ImageMediaId { get; set; }
    // Sender will get this assign in callback
    ImageMessage RcvImageMessage { get; set; }

    public void UploadImageToPeer()
    {
        if (!System.IO.File.Exists(ImagePath))
        {
            string msg = string.Format("File send:{0} does not exist.  Please provide a valid filepath in the Inspector!", ImagePath);
            Debug.Log(msg);
            ShowMessageContent(msg, Message.MessageType.Error);
            return;
        }
        long requestId = 10002;
        int rc = rtmClient.CreateImageMessageByUploading(ImagePath, requestId);

        Debug.LogFormat("Sending image {0} ---> rc={1}", ImagePath, rc);
    }

    public void GetImageByMediaId()
    {
        string mediaID = RcvImageMessage.GetMediaId();
        int rc = rtmClient.DownloadMediaToMemory(mediaID, 100023);
        Debug.LogFormat("Download image {0} ---> rc={1}", mediaID, rc);
    }

    public void SendImageToPeer()
    {
        string peer = peerUserBox.text;
        if (string.IsNullOrEmpty(peer))
        {
            Debug.LogError("You must enter peer id in the input textfield!");
            ShowMessageContent("You must enter peer id in the input textfield!", Message.MessageType.Error);
            return;
        }
        else
        {
            ImageMessage message = rtmClient.CreateImageMessageByMediaId(ImageMediaId);
            rtmClient.SendMessageToPeer(
                peerId: peer,
                message: message,
                options: _MessageOptions
                ); 
        }
    }*/

    #endregion
    #region  -- Invite ---------------------------

    //public void InvitePeer()
    //{
    //    string peerUid = peerUserBox.text;
    //    if (string.IsNullOrEmpty(peerUid))
    //    {
    //        return;
	   // }
    //    // Creates LocalInvitation
    //    LocalInvitation invitation = callManager.CreateLocalCallInvitation(peerUid);
    //    invitation.SetChannelId(ChannelName);
    //    invitation.SetContent("Calling You...hello");
    //    // Sends call invitation
    //    int rc = callManager.SendLocalInvitation(invitation);
    //    Debug.Log("Send invitation to " + peerUid + " rc = " + rc) ;
    //}

    #endregion
    #endregion
    //private void ShowMessageContent(string textStr, Message.MessageType type)
    //{
    //    Debug.Log("RTM:: current messageDisplay is: " + messageDisplay);
    //    if(messageDisplay != null)
    //    {
    //        //messageDisplay.AddTextToDisplay(textStr, type);
    //    }
    //}

    void ShowCurrentChannelName()
    {
        Debug.Log("RTM::Channel name is " + AgoraUtils.GetLocalValue(AgoraConst.CHANNEL_NAME));
    }
    bool ShowDisplayTexts()
    {
        int showLength = 6;
        if (string.IsNullOrEmpty(AgoraConst.APP_ID_RTM) || AgoraConst.APP_ID_RTM.Length < showLength)
        {
            Debug.LogError("App ID is not set, please set it in AgoraConst.cs");
            return false;
        }
        return true;
    }

    #region EventHandlers

    void OnQueryPeersOnlineStatusResultHandler(int id, long requestId, PeerOnlineStatus[] peersStatus, int peerCount, QUERY_PEERS_ONLINE_STATUS_ERR errorCode)
    {
        if (peersStatus.Length > 0)
        {
            Debug.Log("OnQueryPeersOnlineStatusResultHandler requestId = " + requestId +
            " peersStatus: peerId=" + peersStatus[0].peerId +
            " online=" + peersStatus[0].isOnline +
            " onlinestate=" + peersStatus[0].onlineState);
            AgoraDebug.Log("User " + peersStatus[0].peerId + " online status = " + peersStatus[0].onlineState, AgoraDebug.Color.BLUE);
        }
    }

    void OnJoinSuccessHandler(int id)
    {
        string msg = "channel:" + ChannelName + " OnJoinSuccess id = " + id;
        Debug.Log(msg);
        CommonVars.LOCAL_USER_ID = id;
        int requestState = channel.GetMembers();
        DemoGameManager.Instance().InitCharacterModel(AgoraUtils.GetLocalValue(AgoraConst.RTM_USER_NAME));
    }

    void OnJoinFailureHandler(int id, JOIN_CHANNEL_ERR errorCode)
    {
        string msg = "channel OnJoinFailure  id = " + id + " errorCode = " + errorCode;
        Debug.Log(msg);
        AgoraDebug.Log(msg, AgoraDebug.Color.RED);
    }

    void OnClientLoginSuccessHandler(int id)
    {
        string msg = "RTM::client login successful! id = " + id;
        Debug.Log(msg);
 
        JoinChannel();
    }

    void OnClientLoginFailureHandler(int id, LOGIN_ERR_CODE errorCode)
    {
        string msg = "client login unsuccessful! id = " + id + " errorCode = " + errorCode;
        AgoraDebug.Log(msg,AgoraDebug.Color.RED);
        //ShowMessageContent(msg, Message.MessageType.Error);
    }

    void OnLeaveHandler(int id, LEAVE_CHANNEL_ERR errorCode)
    {
        string msg = "client onleave id = " + id + " errorCode = " + errorCode;
        Debug.Log(msg);
 
    }

    void OnChannelMessageReceivedHandler(int id, string userId, TextMessage message)
    {
        Debug.Log("client OnChannelMessageReceived id = " + id + ", from user:" + userId + " text:" + message.GetText());
        //ShowMessageContent(userId + ": " + message.GetText(), Message.MessageType.ChannelMessage);
        DemoGameManager.Instance().DecodeMessage(userId, message.GetText());
    }

    void OnGetMembersHandler(int id, RtmChannelMember[] members, int userCount, GET_MEMBERS_ERR errorCode)
    {
        if (errorCode == GET_MEMBERS_ERR.GET_MEMBERS_ERR_OK)
        {
            AgoraDebug.Log("Total members = " + userCount, AgoraDebug.Color.BLUE);
            foreach (RtmChannelMember member in members)
            {
                AgoraDebug.Log("   member:> " + member.GetUserId(), AgoraDebug.Color.BLUE);
                if(member.GetUserId() != CommonVars.LOCAL_USER_NAME)
                {
                    DemoGameManager.Instance().InitCharacterModel(member.GetUserId(), false);
                }
            }
        }
        else
        {
            AgoraDebug.Log("something is wrong with GetMembers:" + errorCode.ToString(), AgoraDebug.Color.RED);
        }
    }

    void OnMessageReceivedFromPeerHandler(int id, string peerId, TextMessage message)
    {
        Debug.Log("client OnMessageReceivedFromPeer id = " + id + ", from user:" + peerId + " text:" + message.GetText());
        //ShowMessageContent(peerId + ": " + message.GetText(), Message.MessageType.PeerMessage);
    }

    void OnMemberCountUpdatedHandler(int id, int memberCount)
    {
        Debug.Log("Member count changed to:" + memberCount);
    }
    void OnMemberJoinedHandler(int id, RtmChannelMember member)
    {
        Debug.LogFormat("RTM:: OnMemberJoinedHlr playerID : {0} + memberID: {1} + channelID : {2}",id,member.GetUserId(),member.GetChannelId());
        DemoGameManager.Instance().InitCharacterModel(member.GetUserId(),false);

    }

    void OnMemberLeftHandler(int id, RtmChannelMember member)
    {
        string msg = "channel OnMemberLeftHandler member ID=" + member.GetUserId() + " channelId = " + member.GetChannelId();
        Debug.Log(msg);
 
    }


    void OnSendMessageResultHandler(int id, long messageId, PEER_MESSAGE_ERR_CODE errorCode)
    {
        string msg = string.Format("Sent message with id:{0} MessageId:{1} errorCode:{2}", id, messageId, errorCode);
        Debug.Log(msg);
        //ShowMessageContent(msg, errorCode == PEER_MESSAGE_ERR_CODE.PEER_MESSAGE_ERR_OK ?  AgoraDebug.Color.BLUE : Message.MessageType.Error);
    }

    void OnMediaDownloadToFileResultHandler(int id, long requestId, DOWNLOAD_MEDIA_ERR_CODE code)
    {
        Debug.LogFormat("Download id:{0} requestId:{1} errorCode:{2}", id, requestId, code);
    }

    void OnMediaDownloadToMemoryResultHandler(int id, long requestId, byte[] memory, long length, DOWNLOAD_MEDIA_ERR_CODE code)
    {
        Debug.Log("OnMediaDownloadToMemoryResultHandler requestId = " + requestId + " ,length = " + length);
        //messageDisplay.AddImageToDisplay(memory, RcvImageMessage.GetWidth(), RcvImageMessage.GetHight());
        //messageDisplay.AddImageToDisplay(memory);
    }

    void OnConnectionStateChangedHandler(int id, CONNECTION_STATE state, CONNECTION_CHANGE_REASON reason)
    {
        string msg = string.Format("connection state changed id:{0} state:{1} reason:{2}", id, state, reason);
        Debug.Log(msg);
        //ShowMessageContent(msg,  AgoraDebug.Color.BLUE);
    }

    // --------------------------------------
    void OnLocalInvitationReceivedByPeerHandler(LocalInvitation localInvitation)
    {
        string msg = string.Format("OnLocalInvitationReceived channel:{0}, callee:{1}", localInvitation.GetChannelId(), localInvitation.GetCalleeId());
        Debug.Log(msg);
 
	}

    void OnLocalInvitationCanceledHandler(LocalInvitation localInvitation)
    { 
        string msg = string.Format("OnLocalInvitationCanceled channel:{0}, callee:{1}", localInvitation.GetChannelId(), localInvitation.GetCalleeId());
        Debug.Log(msg);
 
	}

    void OnLocalInvitationFailureHandler(LocalInvitation localInvitation, LOCAL_INVITATION_ERR_CODE errorCode)
    {
        string msg = string.Format("OnLocalInvitationFailure channel:{0}, callee:{1} error:{2}", 
		    localInvitation.GetChannelId(), localInvitation.GetCalleeId(), errorCode);
        Debug.Log(msg);
 
    }

    void OnLocalInvitationAcceptedHandler(LocalInvitation localInvitation, string response)
    { 
        string msg = string.Format("OnLocalInvitationAccepted channel:{0}, callee:{1}", localInvitation.GetChannelId(), localInvitation.GetCalleeId());
        Debug.Log(msg);
 
	}
        
    void OnLocalInvitationRefusedHandler(LocalInvitation localInvitation, string response)
    { 
        string msg = string.Format("OnLocalInvitationRefused channel:{0}, callee:{1}", localInvitation.GetChannelId(), localInvitation.GetCalleeId());
        Debug.Log(msg);
 
	
	}
    void OnRemoteInvitationRefusedHandler(RemoteInvitation remoteInvitation)
    { 
        string msg = string.Format("OnRemoteInvitationRefused channel:{0}, callee:{1}", remoteInvitation.GetChannelId(), remoteInvitation.GetCallerId());
        Debug.Log(msg);
 
	}

    void OnRemoteInvitationAcceptedHandler(RemoteInvitation remoteInvitation)
    {
        string msg = string.Format("OnRemoteInvitationAccepted channel:{0}, callee:{1}", remoteInvitation.GetChannelId(), remoteInvitation.GetCallerId());
        Debug.Log(msg);
 
    }

    void OnRemoteInvitationReceivedHandler(RemoteInvitation remoteInvitation)
    {
        string msg = string.Format("OnRemoteInvitationReceived channel:{0}, callee:{1}", remoteInvitation.GetChannelId(), remoteInvitation.GetCallerId());
        Debug.Log(msg);
 
    }
    void OnRemoteInvitationFailureHandler(RemoteInvitation remoteInvitation, REMOTE_INVITATION_ERR_CODE errorCode)
    { 
        string msg = string.Format("OnRemoteInvitationFailure channel:{0}, callee:{1}", remoteInvitation.GetChannelId(), remoteInvitation.GetCallerId());
        Debug.Log(msg);
 
	}
    void OnRemoteInvitationCanceledHandler(RemoteInvitation remoteInvitation)
    { 
        string msg = string.Format("OnRemoteInvitationCanceled channel:{0}, callee:{1}", remoteInvitation.GetChannelId(), remoteInvitation.GetCallerId());
        Debug.Log(msg);
 
	}
    #endregion
}

