using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using Project.Networking;
using Project.Utility;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
namespace BV
{
    public class CharacterSelectorManager : MonoBehaviour
    {
        private ApplicationManager applicationManager;
        private CharacterCreatorSceneManager characterCreatorSceneManager;

        public GameObject selectorPanel;
        public Transform transformSpawnPoint;

        public GameObject charactersListContainer;
        private List<GameObject> listObject = new List<GameObject>();

        private CharacterData[] charactersData;
        private GameObject currentCharacter;

        void Awake()
        {
            applicationManager = ApplicationManager.Instance;
        }

        public void Init()
        {
            if (characterCreatorSceneManager == null)
            {
                characterCreatorSceneManager = CharacterCreatorSceneManager.singleton;
            }

            LoadCharactersList();
            selectorPanel.SetActive(true);
        }

        public void Deinit()
        {
            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }

            for (int i = 0; i < listObject.Count; i++)
            {
                Destroy(listObject[i]);
            }

            listObject = new List<GameObject>();

            selectorPanel.SetActive(false);
        }

        private void LoadCharactersList()
        {
            applicationManager.ShowInformationModal("Триває загрузка персонажів");

            NetworkRequestManager.Instance.EmitWithTimeout(
                "getCharacterList",
                null,
                (response) =>
                    {
                        applicationManager.CloseModal();

                        CharacterListResponse characterListResponse = JsonUtility.FromJson<CharacterListResponse>(response[0].ToString());
                        charactersData = characterListResponse.data;

                        ShowAwaibleCharactersList();
                    },
                (msg) => applicationManager.ShowConfirmationModal(msg)
            );
        }

        private void ShowAwaibleCharactersList()
        {
            GameObject itemTemplate = charactersListContainer.transform.GetChild(0).gameObject;
            GameObject g;

            for (int i = 0; i < charactersData.Length; i++)
            {
                g = Instantiate(itemTemplate, charactersListContainer.transform);
                g.transform.GetChild(0).GetComponent<TMP_Text>().text = charactersData[i].name;
                g.transform.GetChild(1).GetComponent<TMP_Text>().text = charactersData[i].race; ;

                listObject.Add(g);
                g.SetActive(true);
            }

            for (int i = 0; i < listObject.Count; i++)
            {
                listObject[i].GetComponent<Button>().AddEventListener(i, ItemClicked);
            }
        }

        void ItemClicked(int characterIndex)
        {
            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }

            currentCharacter = CharactersController.Instance.CreateCharacter(charactersData[characterIndex], transformSpawnPoint, true);

            if (!currentCharacter)
            {
                return;
            }
        }

        public void OnPlayClick()
        {
            applicationManager.ShowInformationModal("Триває підключення до сервера");

            //@todo pass character id
            NetworkRequestManager.Instance.EmitWithTimeout(
                "selectCharacter",
                null,
                (response) =>
                    {
                        applicationManager.CloseModal();

                        if (SessionManager.Instance != null)
                        {
                            SessionManager.Instance.characterData = new();
                        }
                    },
                (msg) => applicationManager.ShowConfirmationModal(msg)
            );
        }
    }

    [System.Serializable]
    public class CharacterListResponse
    {
        public CharacterData[] data;
        public int length;
    }
}
