using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

namespace BV
{
    public class ChestController : MenuPanel
    {
        public override void Init(SocketIOComponent soc, PlayerData playerData)
        {
            base.Init(soc, playerData);
        }

        public static ChestController singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}