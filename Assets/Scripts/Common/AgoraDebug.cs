using UnityEngine;

public class AgoraDebug
{
    public enum Color
    {
        WHITE,
        RED,
        YELLOW,
        GREEN,
        BLUE
    }
        
    public static void Log(string msg, Color cor = Color.WHITE)
    {
        switch (cor)
        {
            case Color.RED:
                Debug.Log(string.Format("<color=red>{0}</color>", msg));
                break;
            case Color.BLUE:
                Debug.Log(string.Format("<color=blue>{0}</color>", msg));
                break;
            case Color.YELLOW:
                Debug.Log(string.Format("<color=yellow>{0}</color>", msg));
                break;
            case Color.GREEN:
                Debug.Log(string.Format("<color=green>{0}</color>", msg));
                break;
            case Color.WHITE:
            default:
                Debug.Log(msg);
                break;
        }
    }

    public static void ArrLogTypeInt(string  message, int[] target,Color cor = Color.WHITE)
    {
        var displayStr = "";
        for(int i = 0;i<target.Length;i++)
        {
            displayStr += target[i].ToString() + ",";
        }
        Log(message +  displayStr, cor);
    }
}
