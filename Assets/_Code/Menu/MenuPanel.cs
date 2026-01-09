using System.Collections;
using System.Collections.Generic;
using Project.Networking;
using UnityEngine;
using SocketIO;

namespace BV
{
    public class MenuPanel : MonoBehaviour
    {
        [Header("Panel Option")]
        public string panelId;
        public string panelName;

        public virtual void Init(MenuManager menuManager)
        {
        }

        public virtual void Open()
        {
        }

        public virtual void Close()
        {
        }

        public virtual void Deinit()
        {
        }
    }
}
