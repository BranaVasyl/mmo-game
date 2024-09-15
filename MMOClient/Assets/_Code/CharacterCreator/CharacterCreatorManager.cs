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
        private WeatherManager weatherManager;


        private string slectedSex = "man";
        private string slectedRace = "human";
        private GameObject currentCharacter;

        public GameObject creatorPanel;
        public Transform transformSpawnPoint;


        void Start()
        {
            creatorPanel.SetActive(false);

            charactersController = GetComponent<CharactersController>();
            weatherManager = GetComponent<WeatherManager>();

            weatherManager.SetOrbitSbeed(0);

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



        public void OnSetSex(string sex)
        {
            if (slectedSex == sex)
            {
                return;
            }

            slectedSex = sex;
            CreateCharacter();
        }

        public void OnSetRace(string race)
        {
            if (slectedRace == race)
            {
                return;
            }

            slectedRace = race;
            CreateCharacter();
        }

        public void CreateCharacter()
        {
            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }

            string id = slectedSex.ToLower() + char.ToUpper(slectedRace[0]) + slectedRace.Substring(1).ToLower();
            currentCharacter = charactersController.CreateCharacter(id, transformSpawnPoint);
        }

        public void OnPlayClick()
        {
            SceneManager.LoadScene("SampleScene");
        }
    }
}
