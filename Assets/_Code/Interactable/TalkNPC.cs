using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class TalkNPC : MonoBehaviour, IInteractable
    {
        CharacterManager NPCCharacter;
        private GameObject player;

        private DialogManager dialogManager;

        private void Start()
        {
            dialogManager = DialogManager.singleton;
            NPCCharacter = GetComponent<CharacterManager>();
        }

        public string GetDescription()
        {
            return "Поговорити з : " + NPCCharacter.displayedName;
        }

        public void Interact(GameObject character)
        {
            player = character;
            dialogManager.InitDialog(NPCCharacter, character.GetComponent<StateManager>());
        }

        private void Update()
        {
            if (player != null)
            {
                StateManager states = player.GetComponent<StateManager>();

                if (states.inDialog && Vector3.Distance(transform.position, player.transform.position) > 2f)
                {
                    dialogManager.EndDialog();
                }
            }
        }
    }
}
