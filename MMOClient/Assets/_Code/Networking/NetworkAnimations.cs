using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using BV;
using System;

namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))] 
    public class NetworkAnimations : MonoBehaviour
    {
        [SerializeField]
        [GreyOut]
        private float oldVertical;
        [SerializeField]
        [GreyOut]
        private float oldHorizontal;
        [SerializeField]
        [GreyOut]
        private bool oldStateRun;
        [SerializeField]
        [GreyOut]
        private bool oldTwoHanded;
        [SerializeField]
        [GreyOut]
        private bool oldStateWalk;

        //@todo remove this field rom this class
        private NetworkIdentity networkIdentity;
        private PlayerData player;

        private StateManager stateManager;

        void Start()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
            stateManager = GetComponent<StateManager>();
            oldVertical = 0;
            oldHorizontal = 0;
            oldStateRun = false;
            oldStateWalk = false;
            oldTwoHanded = false;

            player = new PlayerData();
            player.vertical = oldVertical;
            player.horizontal = oldHorizontal;

            if (!networkIdentity.IsControlling())
            {
                enabled = false;
            }
        }

        void Update()
        {
            if (networkIdentity.IsControlling())
            {
                if (oldVertical != stateManager.vertical || oldHorizontal != stateManager.horizontal
                || oldStateRun != stateManager.run || oldStateWalk != stateManager.walk || oldTwoHanded != stateManager.isTwoHanded)
                {
                    oldVertical = stateManager.vertical;
                    oldHorizontal = stateManager.horizontal;

                    oldStateRun = stateManager.run;
                    oldStateWalk = stateManager.walk;

                    oldTwoHanded = stateManager.isTwoHanded;

                    sendData();
                }
            }
        }

        private void sendData()
        {
            //Update player information
            player.vertical = Mathf.Round(stateManager.vertical * 1000.0f) / 1000.0f;
            player.horizontal = Mathf.Round(stateManager.horizontal * 1000.0f) / 1000.0f;
            player.run = stateManager.run;
            player.walk = stateManager.walk;
            player.isTwoHanded = stateManager.isTwoHanded;

            SendAnimationsnData sendData = new SendAnimationsnData(player);
            networkIdentity.GetSocket().Emit("updateAnimations", new JSONObject(JsonUtility.ToJson(sendData)));
        }
    }

    [Serializable]
    public class SendAnimationsnData
    {
        public string vertical;
        public string horizontal;
        public bool run;
        public bool walk;
        public bool isTwoHanded;

        public SendAnimationsnData(PlayerData player)
        {
            vertical = player.vertical.ToString();
            horizontal = player.horizontal.ToString();
            run = player.run;
            walk = player.walk;
            isTwoHanded = player.isTwoHanded;
        }
    }
}
