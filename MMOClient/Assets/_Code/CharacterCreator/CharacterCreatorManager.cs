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
        private ApplicationManager applicationManager;
        private CharacterCreatorSceneManager characterCreatorSceneManager;

        public GameObject creatorPanel;
        public Transform transformSpawnPoint;

        private Animator anim;

        private CharacterData characterData = new();
        private GameObject currentCharacter;

        public CharacterData[] characterDatas;
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
                characterData.name = v;
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
            characterData = new();

            creatorPanel.SetActive(false);
        }

        public void OnCreateCharacter()
        {
            applicationManager.ShowInformationModal("Триває створення персонажа");

            NetworkRequestManager.Instance.EmitWithTimeout(
                "createCharacter",
                new JSONObject(JsonUtility.ToJson(characterData)),
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
                );
        }

        public void OnSetGender(string gender)
        {
            if (characterData.gender == gender)
            {
                return;
            }

            characterData.gender = gender;
            CreateCharacter();
        }

        public void OnSetRace(string race)
        {
            if (characterData.race == race)
            {
                return;
            }

            characterData.race = race;
            CreateCharacter();
        }

        public void OnSetClass(string characterClass)
        {
            if (characterData.characterClass == characterClass)
            {
                return;
            }

            characterData.characterClass = characterClass;
            ChangeItems();
        }

        public void OnSetAlliance(string alliance)
        {
            if (characterData.alliance == alliance)
            {
                return;
            }

            characterData.alliance = alliance;
            ChangeItems();
        }

        public void CreateCharacter()
        {
            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }

            currentCharacter = CharactersController.Instance.CreateCharacter(characterData, transformSpawnPoint, false);

            if (!currentCharacter)
            {
                return;
            }

            anim = currentCharacter.GetComponent<Animator>();
            characterModelProvider = currentCharacter.GetComponent<CharacterModelController>();

            ChangeAvaibleCustomizeData();
            characterModelProvider.UpdateCharacterCustomization(characterData.customization);

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
                characterData.customization.hairId = null;
            }
            else
            {
                hairStyleSlider.gameObject.SetActive(true);
                hairStyleSlider.onUpdateData.AddListener(OnUpdateHairStyle);

                hairStyleSlider.SetValue(0);
                hairStyleSlider.SetMaxValue(avaibleCharacterCustomization.hairList.Length - 1);

                int hairIndex = System.Array.FindIndex(avaibleCharacterCustomization.hairList, hair => hair.id == characterData.customization.hairId);
                if (hairIndex >= 0)
                {
                    hairStyleSlider.SetValue(hairIndex);
                }
                else
                {
                    hairStyleSlider.SetValue(0);
                    characterData.customization.hairId = avaibleCharacterCustomization.hairList[0].id;
                }
            }
            //#endregion

            //#region hairStyle
            hairColorPallete.gameObject.SetActive(false);
            hairColorPallete.onUpdateData.RemoveListener(OnUpdateHairColor);
            if (avaibleCharacterCustomization.hairCollorPallete.Length == 0)
            {
                characterData.customization.hairColor = null;
            }
            else
            {
                hairColorPallete.gameObject.SetActive(true);
                hairColorPallete.onUpdateData.AddListener(OnUpdateHairColor);
                hairColorPallete.SetPalette(avaibleCharacterCustomization.hairCollorPallete);

                int colorIndex = System.Array.FindIndex(avaibleCharacterCustomization.hairCollorPallete,
                    color => color.Equals(characterData.customization.hairColor)
                );

                if (colorIndex < 0)
                {
                    string hexColor = ColorUtility.ToHtmlStringRGB(avaibleCharacterCustomization.hairCollorPallete[0]);
                    characterData.customization.hairColor = "#" + hexColor;
                }
            }
            //#endregion
        }

        private void OnUpdateHairStyle(float value)
        {
            int index = Mathf.RoundToInt(value);
            if (index > avaibleCharacterCustomization.hairList.Length || avaibleCharacterCustomization.hairList.Length == 0)
            {
                characterData.customization.hairId = null;
            }
            else
            {
                characterData.customization.hairId = avaibleCharacterCustomization.hairList[index].id;
            }

            characterModelProvider.UpdateCharacterCustomization(characterData.customization);
        }

        private void OnUpdateHairColor(Color color, int index)
        {
            if (index > avaibleCharacterCustomization.hairCollorPallete.Length || avaibleCharacterCustomization.hairCollorPallete.Length == 0)
            {
                characterData.customization.hairColor = null;
            }
            else
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(color);
                characterData.customization.hairColor = "#" + hexColor;
            }

            characterModelProvider.UpdateCharacterCustomization(characterData.customization);
        }

        private void ChangeItems()
        {
            CharacterData? currentInventoryData = null;

            foreach (CharacterData inventoryData in characterDatas)
            {
                if (inventoryData.id.ToString() == characterData.characterClass)
                {
                    currentInventoryData = inventoryData;
                    break;
                }
            }

            PreviewInventoryManager.Instance.SetPlayerEquip(currentInventoryData, currentCharacter);
        }
    }

    [Serializable]
    public class CreateCharacterResponse
    {
        public int code;
        public string msg;
        public CharacterData data;
    }
}
