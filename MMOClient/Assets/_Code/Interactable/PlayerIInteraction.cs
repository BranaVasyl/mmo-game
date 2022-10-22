using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BV
{
    public class PlayerIInteraction : MonoBehaviour
    {
        public Camera mainCamera;
        public float interactioDistance = 2f;

        private GameUIController gameUIController;
        private StateManager states;

        void Start()
        {
            gameUIController = GameUIController.singleton;
            states = GetComponent<StateManager>();
        }

        private void Update()
        {
            InteractionRay();
        }

        void InteractionRay()
        {
            Ray ray = Camera.main.ViewportPointToRay(Vector3.one / 2f);
            RaycastHit hit;

            bool hitSomething = false;

            if (Physics.Raycast(ray, out hit, interactioDistance))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    hitSomething = true;
                    gameUIController.interactionText.text = interactable.GetDescription();

                    if (states.interactInput)
                    {
                        interactable.Interact();
                        states.interactInput = false;
                    }
                }
            }

            gameUIController.interactionUi.SetActive(hitSomething);
        }
    }
}
