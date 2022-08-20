using Project.Utility.Attributes;
using SocketIO;
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

        private SocketIOComponent socket;

        public void Awake()
        {
            isControlling = false; 
        }

        public void SetControllerID (string ID)
        {
            id = ID;
            isControlling = (NetworkClient.ClientID == ID) ? true : false; //Chexk incoming id versuses the one we have saved from the server
        }

        public void SetSocketReference(SocketIOComponent soc)
        {
            socket = soc;
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

        public SocketIOComponent GetSocket ()
        {
            return socket;
        }
    }
}
