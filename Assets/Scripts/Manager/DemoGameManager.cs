using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DemoGameManager : MonoSingleton<DemoGameManager>
{
    //public
    public GameObject LocalPlayer;
    public List<GameObject> mRemotePlayers = new List<GameObject>();
    private Vector3 mStartPostion = new Vector3(0f, 100f, 0f);
    private GameObject mPlayerObjRef;
    private const string PLAYER_REF = "PlayerRef";
    private const string INPUT_BOX = "InputBox";
    private InputField mInputBox;
    void Start()
    {
        mRemotePlayers.Clear();
        LocalPlayer = null;
    }
    public void Init()
    {
        mPlayerObjRef = GameObject.Find(PLAYER_REF);
        mInputBox = GameObject.Find(INPUT_BOX).GetComponent<InputField>();

    }
    public void InitCharacterModel(string playerName,bool isLocal = true)
    {
        UnityEngine.Object animPrefab;
        animPrefab = Resources.Load<GameObject>(AgoraConst.CHARACTER_PREFAB_PATH);
        if(isLocal)
        {
            if(LocalPlayer!= null)
            {
                AgoraDebug.Log("GM::Local user already created");
                return;
            }
            LocalPlayer = Instantiate(animPrefab, mStartPostion, Quaternion.identity) as GameObject;
            LocalPlayer.transform.parent = mPlayerObjRef.transform;
            PlayerController tmpController = LocalPlayer.GetComponent<PlayerController>();
            tmpController.PlayerID = playerName;
            tmpController.IsLocal = true;
            tmpController.SetName (playerName);
        }
        else
        {
            if(!CheckIfRemoteExists(playerName))
            {
                GameObject tmpModel;
                tmpModel = Instantiate(animPrefab, mStartPostion, Quaternion.identity) as GameObject;
                tmpModel.transform.parent = mPlayerObjRef.transform;
                tmpModel.GetComponent<PlayerController>().PlayerID = playerName;
                tmpModel.GetComponent<PlayerController>().IsLocal = false;
                mRemotePlayers.Add(tmpModel);
            }
        }

    }
    public void SendTextMessage()
    {
        LocalPlayer?.GetComponent<PlayerController>().SetDisplayMessage(mInputBox.text);
        SendTextMessage(MessageGenerator.MessageType.Message, mInputBox.text);
    }
    public void SendActionMessage(MessageGenerator.MessageType type,Vector2 inputValue)
    {
        RtmChatManager.Instance().SendMessageToChannel(MessageGenerator.GenerateActionMessage(type, inputValue));
;   }
    public void SendTextMessage(MessageGenerator.MessageType type, string msg)
    {
        RtmChatManager.Instance().SendMessageToChannel(MessageGenerator.GenerateTextMessage(type, msg));
    }
    private bool CheckIfRemoteExists(string playerID)
    {
       return mRemotePlayers.Exists(x => x.GetComponent<PlayerController>().PlayerID == playerID);
    }

    public void DecodeMessage(string playerId, string message)
    {
        if (LocalPlayer == null) return;
        if (playerId == LocalPlayer.GetComponent<PlayerController>().PlayerID) return;
        if(mRemotePlayers.Count > 0)
        {
            int index = mRemotePlayers.FindIndex(x => x.GetComponent<PlayerController>().PlayerID == playerId);
            if(index >=0)
            {
                string[] tmpArr = message.Split(AgoraConst.MESSAGE_SPLITER);
                int currentMessageType;
                int.TryParse(tmpArr[0], out currentMessageType);
                switch((MessageGenerator.MessageType)currentMessageType)
                {
                    case MessageGenerator.MessageType.Action:
                        Vector2 tmpValue = new Vector2();
                        tmpValue.x = float.Parse(tmpArr[1]);
                        tmpValue.y = float.Parse(tmpArr[2]);
                        mRemotePlayers[index].GetComponent<PlayerController>().SetMovement(tmpValue);
                        break;
                    case MessageGenerator.MessageType.Message:
                        mRemotePlayers[index].GetComponent<PlayerController>().SetDisplayMessage(tmpArr[1]);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

