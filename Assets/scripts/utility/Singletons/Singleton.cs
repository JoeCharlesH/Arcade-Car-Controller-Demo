using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _i;
    private static object _lock = new object();
    private static bool appIsQuitting = false;

    public static T instance
    {
        get {
            if (appIsQuitting) {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }

            lock (_lock) {
                if (_i == null) {
                    _i = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1) {
                        Debug.LogError("[Singleton] More than one Singleton found! Reopenning the scene might fix it.");
                        return _i;
                    }

                    if (_i == null) {
                        GameObject singleton = new GameObject();
                        _i = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();

                        DontDestroyOnLoad(singleton);

                        Debug.Log("[Singleton] Created singleton of type " + typeof(T) + ": " + singleton);
                    }
                }

                return _i;
            }
        }
    }
    public virtual void OnDestroy() {
        appIsQuitting = true;
    }
}
