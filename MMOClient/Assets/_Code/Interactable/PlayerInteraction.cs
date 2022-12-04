using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Project.Networking;

namespace BV
{
    public class PlayerInteraction : MonoBehaviour
    {
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
            List<GameObject> interactableGameObject = new List<GameObject>();
            Collider[] colliderArray = Physics.OverlapSphere(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), interactioDistance);
            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent(out IInteractable interactable))
                {
                    interactableGameObject.Add(collider.gameObject);
                }
            }

            GameObject closestGameObject = null;
            foreach (GameObject interactable in interactableGameObject)
            {
                if (closestGameObject == null)
                {
                    closestGameObject = interactable;
                }
                else
                {
                    if (Vector3.Distance(transform.position, interactable.transform.position) < Vector3.Distance(transform.position, closestGameObject.transform.position))
                    {
                        closestGameObject = interactable;
                    }
                }
            }

            bool hitSomething = false;
            if (closestGameObject != null)
            {
                IInteractable currentInteractable = closestGameObject.GetComponent<IInteractable>();
                if (currentInteractable != null)
                {
                    hitSomething = true;
                    gameUIManager.interactionText.text = currentInteractable.GetDescription();

                    if (states.interactInput)
                    {
                        currentInteractable.Interact(gameObject);
                        states.interactInput = false;
                    }
                }
            }

            if (hitSomething)
            {
                gameUIManager.ShowInteractionUI();
            }
            else
            {
                gameUIManager.HideInteractionUI();
            }
        }
    }
}
