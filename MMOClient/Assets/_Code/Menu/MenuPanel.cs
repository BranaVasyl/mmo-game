using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;
using SocketIO;

namespace BV
{
    public class MenuPanel : MonoBehaviour
    {
        [Header("Panel Option")]
        public string panelId;
        public string panelName;

        public virtual void Init(SocketIOComponent soc, PlayerData pD)
        {
        }

        public virtual void Open()
        {
        }

        public virtual void Deinit()
        {
        }
    }
}
