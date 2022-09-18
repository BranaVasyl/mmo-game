using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class NPCDialog : MonoBehaviour
    {
        public string dialogName;
        private DialogManager dialogManager;

        void Awake() {
            dialogManager = DialogManager.singleton;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                NetworkIdentity ni = other.gameObject.GetComponent<NetworkIdentity>();
                if (ni.IsControlling())
                {
                    dialogManager.InitDialog("DialogText/" + dialogName, other.gameObject.GetComponent<StateManager>());
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                NetworkIdentity ni = other.gameObject.GetComponent<NetworkIdentity>();
                if (ni.IsControlling())
                {
                    dialogManager.EndDialog();
                }
            }
        }
    }
}
