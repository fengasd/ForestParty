using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgoraUtils
{
    // Start is called before the first frame update
    public static void SaveLocalValue(string objName,string ObjValue)
    {
        PlayerPrefs.SetString(objName, ObjValue);
        PlayerPrefs.Save();
    }

    public static string GetLocalValue(string objName)
    {
        return PlayerPrefs.GetString(objName);
    }
}
