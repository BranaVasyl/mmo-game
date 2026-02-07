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
        public PreviewInventoryManager previewInventoryManager;

        private ApplicationManager applicationManager;
        private CharacterCreatorSceneManager characterCreatorSceneManager;

        public GameObject selectorPanel;
        public Transform transformSpawnPoint;

        public GameObject charactersListContainer;
        private List<GameObject> listObject = new List<GameObject>();

        private PlayerData[] playersData;
        private PlayerData currentPlayerData;
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
            if (currentPlayerData != null)
            {
                currentPlayerData = null;
            }

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
                new NetworkEvent(
                    "getCharacterList",
                    null,
                    (response) =>
                        {
                            applicationManager.CloseModal();

                            PlayerListResponse playerListResponse = JsonUtility.FromJson<PlayerListResponse>(response[0].ToString());
                            playersData = playerListResponse.data;

                            if (playersData.Length > 0)
                            {
                                ShowAwaibleCharactersList();
                                ItemClicked(0);
                            }
                        },
                    (msg) => applicationManager.ShowConfirmationModal(msg)
                )
            );
        }

        private void ShowAwaibleCharactersList()
        {
            GameObject itemTemplate = charactersListContainer.transform.GetChild(0).gameObject;
            GameObject g;

            for (int i = 0; i < playersData.Length; i++)
            {
                g = Instantiate(itemTemplate, charactersListContainer.transform);
                g.transform.GetChild(0).GetComponent<TMP_Text>().text = playersData[i].name;
                g.transform.GetChild(1).GetComponent<TMP_Text>().text = playersData[i].race; ;

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
            if (currentPlayerData != null && playersData[characterIndex].id == currentPlayerData.id)
            {
                return;
            }

            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }

            currentPlayerData = playersData[characterIndex];
            currentCharacter = CharactersController.Instance.CreateCharacter(currentPlayerData, transformSpawnPoint);

            previewInventoryManager.SetPlayerEquip(currentPlayerData, currentCharacter);
        }

        public void OnPlayClick()
        {
            if (currentPlayerData == null)
            {
                return;
            }

            applicationManager.ShowInformationModal("Триває підключення до сервера");

            JSONObject selectPlayerData = new();
            selectPlayerData.AddField("id", currentPlayerData.id);

            //@todo pass character id
            NetworkRequestManager.Instance.EmitWithTimeout(
                new NetworkEvent(
                    "selectCharacter",
                    selectPlayerData,
                    (response) =>
                        {
                            applicationManager.CloseModal();

                            SelectCharacterResponse responseData = JsonUtility.FromJson<SelectCharacterResponse>(response[0].ToString());

                            if (responseData.code != 0)
                            {
                                string text = "";
                                switch (responseData.code)
                                {
                                    case 1:
                                        text = "Не вдалося вибрати даного персонажа, спробуй іншого";
                                        break;
                                    default:
                                        text = "Щось пішло не так :(";
                                        break;
                                }

                                applicationManager.ShowConfirmationModal(text);
                                return;
                            };
                        },
                    (msg) => applicationManager.ShowConfirmationModal(msg)
                )
            );
        }
    }

    [Serializable]
    public class SelectCharacterResponse
    {
        public int code;
        public string msg;
    }

    [System.Serializable]
    public class PlayerListResponse
    {
        public PlayerData[] data;
        public int length;
    }
}
