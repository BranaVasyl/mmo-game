using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Project.Networking;
using TMPro;

namespace BV
{
    public class CharacterCreatorManager : MonoBehaviour
    {
        public PreviewInventoryManager previewInventoryManager;

        private ApplicationManager applicationManager;
        private CharacterCreatorSceneManager characterCreatorSceneManager;

        public GameObject creatorPanel;
        public Transform transformSpawnPoint;

        private Animator anim;

        private PlayerData playerData = new();
        private GameObject currentCharacter;

        public PlayerData[] playerDatas;
        private CharacterModelController characterModelProvider;
        private GameObject rightHandObject;
        private GameObject leftHandObject;

        [Header("Character Customization")]
        private AvaibleCharacterCustomization avaibleCharacterCustomization;

        public TMP_InputField characterNameInputField;
        public StepLider hairStyleSlider;
        public ColorPallete hairColorPallete;

        void Awake()
        {
            applicationManager = ApplicationManager.Instance;
            characterCreatorSceneManager = CharacterCreatorSceneManager.singleton;
        }

        void Start()
        {
            characterNameInputField.onValueChanged.AddListener((v) =>
            {
                playerData.name = v;
            });
        }

        public void Init()
        {
            CreateCharacter();
            creatorPanel.SetActive(true);
        }

        public void Deinit()
        {
            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }
            characterNameInputField.text = "";
            playerData = new();

            creatorPanel.SetActive(false);
        }

        public void OnCreateCharacter()
        {
            applicationManager.ShowInformationModal("Триває створення персонажа");

            NetworkRequestManager.Instance.EmitWithTimeout(
                new NetworkEvent(
                    "createCharacter",
                    new JSONObject(JsonUtility.ToJson(playerData)),
                    (response) =>
                        {
                            applicationManager.CloseModal();

                            CreateCharacterResponse responseData = JsonUtility.FromJson<CreateCharacterResponse>(response[0].ToString());

                            if (responseData.code != 0)
                            {
                                string text = "";
                                switch (responseData.code)
                                {
                                    case 1:
                                        text = "Не вдалося додати персонажа до цього аккаунта, вийди з гри і повтори ще раз";
                                        break;
                                    case 2:
                                        text = "Не вдалося створити персонажа";
                                        break;
                                    case 3:
                                        text = responseData.msg;
                                        break;
                                    default:
                                        text = "Щось пішло не так :(";
                                        break;
                                }

                                applicationManager.ShowConfirmationModal(text);
                                return;
                            };

                            characterCreatorSceneManager.OnGoSelectCharacterMode();
                        },
                    (msg) => applicationManager.ShowConfirmationModal(msg)
                )
            );
        }

        public void OnSetGender(string gender)
        {
            if (playerData.gender == gender)
            {
                return;
            }

            playerData.gender = gender;
            CreateCharacter();
        }

        public void OnSetRace(string race)
        {
            if (playerData.race == race)
            {
                return;
            }

            playerData.race = race;
            CreateCharacter();
        }

        public void OnSetClass(string characterClass)
        {
            if (playerData.characterClass == characterClass)
            {
                return;
            }

            playerData.characterClass = characterClass;
            ChangeItems();
        }

        public void OnSetAlliance(string alliance)
        {
            if (playerData.alliance == alliance)
            {
                return;
            }

            playerData.alliance = alliance;
            ChangeItems();
        }

        public void CreateCharacter()
        {
            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }

            currentCharacter = CharactersController.Instance.CreateCharacter(playerData, transformSpawnPoint, false);

            if (!currentCharacter)
            {
                return;
            }

            anim = currentCharacter.GetComponent<Animator>();
            characterModelProvider = currentCharacter.GetComponent<CharacterModelController>();

            ChangeAvaibleCustomizeData();
            characterModelProvider.UpdateCharacterCustomization(playerData.customization);

            ChangeItems();
        }

        private void ChangeAvaibleCustomizeData()
        {
            //@todo request to server
            avaibleCharacterCustomization = characterModelProvider.GetAvaibleCharacterCustomization();

            //#region hairStyle
            hairStyleSlider.gameObject.SetActive(false);
            hairStyleSlider.onUpdateData.RemoveListener(OnUpdateHairStyle);
            if (avaibleCharacterCustomization.hairList.Length == 0)
            {
                playerData.customization.hairId = null;
            }
            else
            {
                hairStyleSlider.gameObject.SetActive(true);
                hairStyleSlider.onUpdateData.AddListener(OnUpdateHairStyle);

                hairStyleSlider.SetValue(0);
                hairStyleSlider.SetMaxValue(avaibleCharacterCustomization.hairList.Length - 1);

                int hairIndex = System.Array.FindIndex(avaibleCharacterCustomization.hairList, hair => hair.id == playerData.customization.hairId);
                if (hairIndex >= 0)
                {
                    hairStyleSlider.SetValue(hairIndex);
                }
                else
                {
                    hairStyleSlider.SetValue(0);
                    playerData.customization.hairId = avaibleCharacterCustomization.hairList[0].id;
                }
            }
            //#endregion

            //#region hairStyle
            hairColorPallete.gameObject.SetActive(false);
            hairColorPallete.onUpdateData.RemoveListener(OnUpdateHairColor);
            if (avaibleCharacterCustomization.hairCollorPallete.Length == 0)
            {
                playerData.customization.hairColor = null;
            }
            else
            {
                hairColorPallete.gameObject.SetActive(true);
                hairColorPallete.onUpdateData.AddListener(OnUpdateHairColor);
                hairColorPallete.SetPalette(avaibleCharacterCustomization.hairCollorPallete);

                int colorIndex = System.Array.FindIndex(avaibleCharacterCustomization.hairCollorPallete,
                    color => color.Equals(playerData.customization.hairColor)
                );

                if (colorIndex < 0)
                {
                    string hexColor = ColorUtility.ToHtmlStringRGB(avaibleCharacterCustomization.hairCollorPallete[0]);
                    playerData.customization.hairColor = "#" + hexColor;
                }
            }
            //#endregion
        }

        private void OnUpdateHairStyle(float value)
        {
            int index = Mathf.RoundToInt(value);
            if (index > avaibleCharacterCustomization.hairList.Length || avaibleCharacterCustomization.hairList.Length == 0)
            {
                playerData.customization.hairId = null;
            }
            else
            {
                playerData.customization.hairId = avaibleCharacterCustomization.hairList[index].id;
            }

            characterModelProvider.UpdateCharacterCustomization(playerData.customization);
        }

        private void OnUpdateHairColor(Color color, int index)
        {
            if (index > avaibleCharacterCustomization.hairCollorPallete.Length || avaibleCharacterCustomization.hairCollorPallete.Length == 0)
            {
                playerData.customization.hairColor = null;
            }
            else
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(color);
                playerData.customization.hairColor = "#" + hexColor;
            }

            characterModelProvider.UpdateCharacterCustomization(playerData.customization);
        }

        private void ChangeItems()
        {
            PlayerData? currentInventoryData = null;

            foreach (PlayerData inventoryData in playerDatas)
            {
                if (inventoryData.id.ToString() == playerData.characterClass)
                {
                    currentInventoryData = inventoryData;
                    break;
                }
            }

            previewInventoryManager.SetPlayerEquip(currentInventoryData, currentCharacter);
        }
    }

    [Serializable]
    public class CreateCharacterResponse
    {
        public int code;
        public string msg;
        public PlayerData data;
    }
}
