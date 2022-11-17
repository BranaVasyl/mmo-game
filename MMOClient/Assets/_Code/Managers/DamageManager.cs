using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;

namespace BV
{
    public class DamageManager : MonoBehaviour
    {
        private ManagersController managersController;
        public List<KillListener> killListeners;

        public void Init(ManagersController mC)
        {
            managersController = mC;
        }

        public void CreatateDamageEvent(CharacterManager agent, CharacterManager target, float damage)
        {
            if (agent.networkIdentity.IsControlling() && target.canDoDamage())
            {
                target.DoDamage();
                SendDamageData sendData = new SendDamageData(agent.networkIdentity.GetID(), target.networkIdentity.GetID(), damage);
                managersController.socket.Emit("doDamage", new JSONObject(JsonUtility.ToJson(sendData)));
            }
        }

        public void OnKillEvent(string id)
        {
            KillListener killListener = killListeners.Find(item => item.enemyId == id);
            if (killListener == null)
            {
                return;
            }

            killListener.OnComlete();
        }

        public static DamageManager singleton;
        void Awake()
        {
            singleton = this;
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

    [System.Serializable]
    public class KillListener
    {
        public string enemyId;
        public bool completed;
        public List<QuestEvent> eventList;

        public void OnComlete()
        {
            for (int i = 0; i < eventList.Count; i++)
            {
                eventList[i].TriggerEvent();
            }

            completed = true;
        }
    }
}
