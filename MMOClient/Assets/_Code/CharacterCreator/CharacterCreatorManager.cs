using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BV
{
    [RequireComponent(typeof(CharactersController))]
    public class CharacterCreatorManager : MonoBehaviour
    {
        private CharactersController charactersController;
        private GameObject currentCharacter;

        public GameObject creatorPanel;
        public Transform transformSpawnPoint;


        void Start()
        {
            creatorPanel.SetActive(false);
            charactersController = GetComponent<CharactersController>();
            GoDefaultMode();
        }

        public void GoCharacterCreatorMode()
        {
            creatorPanel.SetActive(true);
            CreateCharacter();
        }

        public void LeaveCharacterCreatorMode()
        {
            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }
            creatorPanel.SetActive(false);

            GoDefaultMode();
        }

        public void GoDefaultMode()
        {
        }

        public void CreateCharacter(string id = "manHuman")
        {
            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }

            currentCharacter = charactersController.CreateCharacter(id, transformSpawnPoint);
        }

        public void OnPlayClick()
        {
            SceneManager.LoadScene("SampleScene");
        }
    }
}
