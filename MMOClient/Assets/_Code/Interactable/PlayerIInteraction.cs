using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Project.Networking;

namespace BV
{
    public class PlayerIInteraction : MonoBehaviour
    {
        public Camera mainCamera;
        public float interactioDistance = 1.2f;

        private GameUIManager gameUIManager;
        private StateManager states;

        private bool canInteract = false;

        void Start()
        {
            gameUIManager = GameUIManager.singleton;
            states = GetComponent<StateManager>();
            canInteract = gameObject.GetComponent<NetworkIdentity>().IsControlling();
        }

        private void Update()
        {
            if (canInteract)
            {
                InteractionRay();
            }
        }

        void InteractionRay()
        {
            List<IInteractable> interactableList = new List<IInteractable>();
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactioDistance);
            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent(out IInteractable interactable))
                {
                    interactableList.Add(interactable);
                }
            }

            IInteractable closestInteractable = null;
            foreach (IInteractable interactable in interactableList)
            {
                if (closestInteractable == null)
                {
                    closestInteractable = interactable;
                }
            }


            bool hitSomething = false;
            if (closestInteractable != null)
            {
                hitSomething = true;
                gameUIManager.interactionText.text = closestInteractable.GetDescription();

                if (states.interactInput)
                {
                    closestInteractable.Interact(gameObject);
                    states.interactInput = false;
                }
            }

            gameUIManager.interactionUi.SetActive(hitSomething);
        }
    }
}
