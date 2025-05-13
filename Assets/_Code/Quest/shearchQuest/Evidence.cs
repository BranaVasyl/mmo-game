using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class Evidence : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                NetworkIdentity ni = other.gameObject.GetComponent<NetworkIdentity>();
                if (ni.IsControlling())
                {
                    Debug.Log("Evidence");
                    gameObject.transform.parent.gameObject.GetComponent<ReshearchArea>().UpdateAreaEvidence(gameObject);
                    Destroy(GetComponent<Evidence>());
                }
            }
        }
    }
}