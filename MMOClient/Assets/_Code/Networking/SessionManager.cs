using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class SessionManager : MonoBehaviour
    {
        public CharacterData characterData;

        private static SessionManager instance;

        public static SessionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("SessionManager not found. Make sure it exists in the scene.");
                }
                return instance;
            }
        }

        void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}
