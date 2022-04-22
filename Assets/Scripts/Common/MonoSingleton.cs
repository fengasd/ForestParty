using UnityEngine;

/// <summary>
/// 需要使用Unity生命周期的单例模式
/// </summary>
public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance = null;
    public static T Instance()
    {
        if (_instance != null)
        {
            return _instance;
        }
        
        _instance = FindObjectOfType<T>();
        if (FindObjectsOfType<T>().Length > 1)
        {
            AgoraDebug.Log($"More than 1!!");
            return _instance;
        }
            
        if (_instance == null)
        {
            var instanceName = typeof(T).Name;
            AgoraDebug.Log($"Instance Name : " + instanceName);
            var go = GameObject.Find(instanceName);
            if (go == null)
            {
                go = new GameObject(instanceName);
            }

            _instance = go.AddComponent<T>();
            DontDestroyOnLoad(go);
            AgoraDebug.Log($"Add New Singleton {instanceName} In Game!!");
        }
        else
        {
            AgoraDebug.Log($"{_instance.name} Already Exist!");
        }
        
        return _instance;
    }

    private void OnDestroy()
    {
        _instance = null;
    }
}
