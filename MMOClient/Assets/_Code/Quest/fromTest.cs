using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{

    public class fromTest : MonoBehaviour
    {
        public List<QuestEvent> questEvents;
        public bool removeAfterUse = true;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                NetworkIdentity ni = other.gameObject.GetComponent<NetworkIdentity>();
                if (ni.IsControlling())
                {
                    for (int i = 0; i < questEvents.Count; i++)
                    {
                        questEvents[i].TriggerEvent();
                    }

                    if (removeAfterUse)
                    {
                        Destroy(this.gameObject);
                    }
                }
            }
        }
    }
}