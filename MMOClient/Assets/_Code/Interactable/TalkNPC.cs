using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class TalkNPC : MonoBehaviour, IInteractable
    {
        public string NPCId;
        public string NPCName;

        private DialogManager dialogManager;

        private void Start()
        {
            dialogManager = DialogManager.singleton;
        }

        public string GetDescription()
        {
            return "Поговорити з : " + NPCName;
        }

        public void Interact(GameObject player)
        {
            dialogManager.InitDialog(NPCId, player.GetComponent<StateManager>());
        }

        private void OnTriggerExit(Collider other)
        {

            if (other.gameObject.tag == "Player")
            {
                NetworkIdentity ni = other.gameObject.GetComponent<NetworkIdentity>();
                StateManager states = other.gameObject.GetComponent<StateManager>();

                if (ni.IsControlling() && states.inDialog)
                {
                    dialogManager.EndDialog();
                }
            }
        }
    }
}
