using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance == null)
                    {
                        Debug.LogError(typeof(T).ToString() + " not found. Make sure it exists in the scene.");
                    }
                }
                return instance;
            }
        }

        public virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                DestroyImmediate(this);
                return;
            }

            instance = this as T;

            if (transform.parent == null)
            {
                DontDestroyOnLoad(this);
            }
        }
    }
}