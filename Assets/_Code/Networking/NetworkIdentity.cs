using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Networking {
    public class NetworkIdentity : MonoBehaviour
    {
        [Header("Helpful Values")]
        [SerializeField]
        [GreyOut]
        private string id;
        [SerializeField]
        [GreyOut]
        private bool isControlling;
        [SerializeField]
        [GreyOut]
        private JSONObject meta;

        public void Awake()
        {
            isControlling = false; 
        }

        public void SetControllerID (string ID)
        {
            id = ID;
        }

        public string GetID() 
        {
            return id;
        }

        public void setIsControling(bool isControl) {
            isControlling = isControl;
        }

        public bool IsControlling()
        {
            return isControlling;
        }

        public void SetMeta(JSONObject metaData)
        {
            meta = metaData;
        }

        public JSONObject GetMeta()
        {
            return meta;
        }
    }
}
