using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Danbi
{
    /// <summary>
    /// Live along the application life-time. Add as s aGameObject automatically.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonAsComponent<T> : MonoBehaviour where T : SingletonAsComponent<T>, new()
    {
        static T m_instance;
        public static T instance => CreateOrGetInstance();

        static T CreateOrGetInstance()
        {
            //
            if (m_instance is null)
            {
                // 1. Search for the singleton already exists.
                var mgr = FindObjectsOfType<T>();

                if (!(mgr is null))
                {
                    if (mgr.Length == 1)
                    {
                        m_instance = mgr[0];
                        return m_instance;
                    }
                    else if (mgr.Length > 1)
                    {
                        // Delete existing singleton objects.
                        for (int i = 0; i < mgr.Length; ++i)
                        {
                            Destroy(mgr[i].gameObject);
                        }
                    }
                }

                // 3. Create new Singleton GameObject.
                var singletonGo = new GameObject(typeof(T).Name + $"_autocreated_sigleton", typeof(T));
                m_instance = singletonGo.GetComponent<T>();
                DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
            }

        void OnApplicationQuit()
        {
            DanbiUISync.UnbindAll();
            Destroy(gameObject);
        }
    };
};
