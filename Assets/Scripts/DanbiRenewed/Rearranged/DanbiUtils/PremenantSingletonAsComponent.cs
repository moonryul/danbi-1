using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Danbi
{
    /// <summary>
    /// Live along the application life-time. Add as s aGameObject automatically.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PremenantSingletonAsComponent<T> : MonoBehaviour
        where T : PremenantSingletonAsComponent<T>, new()
    {
        static T Instance;
        public static T instance => CreateOrGetInstance();

        static T CreateOrGetInstance()
        {
            //
            if (Instance.Null())
            {
                // 1. Search for the singleton already exists.
                var mgr = FindObjectsOfType<T>();

                // 2. If manager has been found 
                //    and it's only 1.
                if (mgr?.Length == 1)
                {
                    Instance = mgr[0];
                    // Debug.Log($"<color=green>Singleton <<{typeof(T).Name} already exists. you are using this.>></color>");
                    return Instance;

                }
                else if (mgr?.Length > 1)
                {
                    Debug.LogError($"<color=yellow>You cannot have more than one {typeof(T).Name} in this scene. You just need only one. all of them are deleted!</color>");
                    // Delete existing singleton objects.
                    for (int i = 0; i < mgr?.Length; ++i)
                    {
                        var fwd = mgr[i];
                        DestroyImmediate(fwd?.gameObject);
                    }
                }

                // 3. Create new Singleton GameObject.
                var singletonGo = new GameObject(typeof(T).Name + $"_autocreated_sigleton",
                                                 typeof(T) ?? null);
                Instance = singletonGo?.GetComponent<T>();
                DontDestroyOnLoad(Instance?.gameObject);
            }
            return Instance;
        }

        void OnApplicationQuit()
        {
            Destroy(gameObject);
        }
    };
};
