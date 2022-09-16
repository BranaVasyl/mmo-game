using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;

namespace BV
{
    public class DamageManager : MonoBehaviour
    {
        private SocketIOComponent socket;

        public static DamageManager singleton;
        void Awake()
        {
            singleton = this;
        }

        public void Init(SocketIOComponent soc)
        {
            socket = soc;
        }

        public void CreatateDamageEvent(CharacterManager agent, CharacterManager target, float damage)
        {
            if (target.networkIdentity.IsControlling() && target.canDoDamage())
            {
                target.DoDamage();
                SendDamageData sendData = new SendDamageData(agent.networkIdentity.GetID(), target.networkIdentity.GetID(), damage);
                socket.Emit("doDamage", new JSONObject(JsonUtility.ToJson(sendData)));
            }
        }
    }

    [Serializable]
    public class SendDamageData
    {
        public string agentId;
        public string targetId;
        public string damage;

        public SendDamageData(string aId, string tId, float dam)
        {
            agentId = aId;
            targetId = tId;
            damage = dam.ToString();
        }
    }
}
