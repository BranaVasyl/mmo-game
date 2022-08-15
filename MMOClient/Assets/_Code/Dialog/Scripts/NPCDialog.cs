using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class NPCDialog : MonoBehaviour
    {
        public string dialogName;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                NetworkIdentity ni = other.gameObject.GetComponent<NetworkIdentity>();
                if (ni.IsControlling())
                {
                    GameObject dialogManager = GameObject.FindGameObjectWithTag("Dialog_Manager");
                    dialogManager.GetComponent<DialogManager>().InitDialog("DialogText/" + dialogName);
                }
            }
        }
    }
}
