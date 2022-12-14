using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class CharacterManager : MonoBehaviour
    {
        [Header("Character Stats")]
        public NetworkIdentity networkIdentity;
        public float health = 100;
        public string displayedName;
        public string id;

        public bool isDead = false;

        public float money = 0;

        public void Init(NetworkIdentity nI)
        {
            networkIdentity = nI;
        }

        public virtual bool canDoDamage()
        {
            return false;
        }

        public virtual void DoDamage()
        {
        }
    }
}
