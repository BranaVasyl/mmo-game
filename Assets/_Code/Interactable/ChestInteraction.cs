using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class ChestInteraction : MonoBehaviour, IInteractable
    {
        public string interactLabel = "Відкрити судук";

        public string GetDescription()
        {
            return interactLabel;
        }

        public void Interact(GameObject player)
        {
            var ni = GetComponent<NetworkIdentity>();
            if (ni == null) {
                return;
            }

            var meta = ni.GetMeta();
            if (meta == null) {
                return;
            }

            if (meta.HasField("storageId"))
            {
                string storageId = meta["storageId"].str;
                MenuManager.singleton.OpenChest(storageId);
            }
        }
    }
}
