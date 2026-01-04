using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class BagInteraction : MonoBehaviour, IInteractable
    {
        public string interactLabel = "Відкрити сумку";
        private GameObject player;

        public string GetDescription()
        {
            return interactLabel;
        }

        public void Interact(GameObject character)
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
                player = character;

                string storageId = meta["storageId"].str;
                PickUpManager.singleton.OpenBag(storageId);
            }
        }

        private void Update()
        {
            if (player != null)
            {
                if (Vector3.Distance(transform.position, player.transform.position) > 2)
                {
                    PickUpManager.singleton.CloseBag();
                    player = null;
                }
            }
        }
    }
}
