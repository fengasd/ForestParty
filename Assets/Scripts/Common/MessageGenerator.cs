using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageGenerator 
{
    // Start is called before the first frame update
    public enum MessageType
    {
        Action = 0,
        Message
    }
    public static string GenerateActionMessage( MessageType type,Vector2 inputValue)
    {
        string returnStr = "";
        returnStr = type.ToString() + AgoraConst.MESSAGE_SPLITER
                    + $"{inputValue.x:0.##}" + AgoraConst.MESSAGE_SPLITER
                    + $"{inputValue.y:0.##}";
        return returnStr;
    }
    public static string GenerateTextMessage(MessageType type, string message)
    {
        string returnStr = "";
        returnStr = type.ToString() + AgoraConst.MESSAGE_SPLITER + message;
        return returnStr;
    }
}
