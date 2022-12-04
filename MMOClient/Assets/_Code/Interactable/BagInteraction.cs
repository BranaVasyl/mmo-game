using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class BagInteraction : MonoBehaviour, IInteractable
    {
        public string bagId = "3";
        private GameObject player;

        public string GetDescription()
        {
            return "Відкрити сумку";
        }

        public void Interact(GameObject character)
        {
            player = character;
            PickUpManager.singleton.OpenBag(bagId, gameObject);
        }

        private void Update()
        {
            if (player != null)
            {
                if (Vector3.Distance(transform.position, player.transform.position) > 2f)
                {
                    PickUpManager.singleton.ClsoeBag();
                }
            }
        }
    }
}
