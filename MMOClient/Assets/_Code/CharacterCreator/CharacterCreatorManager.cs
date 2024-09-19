using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

namespace BV
{
    [RequireComponent(typeof(CharactersController))]
    public class CharacterCreatorManager : MonoBehaviour
    {
        private CharactersController charactersController;
        private WeatherManager weatherManager;
        private Animator anim;

        private CharacterData characterData = new();
        private GameObject currentCharacter;

        public GameObject creatorPanel;
        public Transform transformSpawnPoint;

        public CharacterCreatorInventoryData[] characterCreatorInventoryDatas;
        private CharacterModelController characterModelProvider;
        private GameObject rightHandObject;
        private GameObject leftHandObject;
        public AvaibleCharacterCustomization avaibleCharacterCustomization;

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


        public void OnNextHair()
        {
            if (avaibleCharacterCustomization.hairList.Length == 0)
            {
                characterData.characterCustomizationData.hairId = null;
            }
            else
            {
                if (characterData.characterCustomizationData.hairId == null)
                {
                    characterData.characterCustomizationData.hairId = avaibleCharacterCustomization.hairList[0].id;
                }
                else
                {
                    for (int i = 0; i < avaibleCharacterCustomization.hairList.Length; i++)
                    {
                        if (avaibleCharacterCustomization.hairList[i].id == characterData.characterCustomizationData.hairId)
                        {
                            if (i < avaibleCharacterCustomization.hairList.Length - 1)
                            {
                                characterData.characterCustomizationData.hairId = avaibleCharacterCustomization.hairList[i + 1].id;
                            }
                            else
                            {
                                characterData.characterCustomizationData.hairId = avaibleCharacterCustomization.hairList[0].id;
                            }

                            characterModelProvider.UpdateCharacterCustomization(characterData.characterCustomizationData);
                            return;
                        }
                    }
                }
            }

            characterModelProvider.UpdateCharacterCustomization(characterData.characterCustomizationData);
        }

        public void CreateCharacter()
        {
            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }

            currentCharacter = charactersController.CreateCharacter(characterData, transformSpawnPoint, false);

            if (!currentCharacter)
            {
                return;
            }

            anim = currentCharacter.GetComponent<Animator>();
            characterModelProvider = currentCharacter.GetComponent<CharacterModelController>();

            ChangeAvaibleCustomizeData();
            ChangeItems();
        }

        private void ChangeAvaibleCustomizeData()
        {
            //@todo request to server
            avaibleCharacterCustomization = characterModelProvider.GetAvaibleCharacterCustomization();

            if (avaibleCharacterCustomization.hairList.Length == 0)
            {
                characterData.characterCustomizationData.hairId = null;
            }
            else
            {
                if (System.Array.Find(avaibleCharacterCustomization.hairList, hair => hair.id == characterData.characterCustomizationData.hairId) == null)
                {
                    characterData.characterCustomizationData.hairId = avaibleCharacterCustomization.hairList.Length > 0 ? avaibleCharacterCustomization.hairList[0].id : null;
                }
            }
        }

        public void OnPlayClick()
        {
            SceneManager.LoadScene("SampleScene");
        }

        #region change character items
        private void ChangeItems()
        {
            CharacterCreatorInventoryData? currentInventoryData = null;

            foreach (CharacterCreatorInventoryData inventoryData in characterCreatorInventoryDatas)
            {
                if (inventoryData.id.ToString() == characterData.characterClass)
                {
                    currentInventoryData = inventoryData;
                    break;
                }
            }

            if (anim != null)
            {
                ChangeRightHandItem(currentInventoryData?.rightHandItem ?? null);
                ChangeLeftHandItem(currentInventoryData?.leftHandItem ?? null);
                HandleTwoHanded(currentInventoryData.isTwoHadned, currentInventoryData);
            }
        }

        private void ChangeRightHandItem(ItemWeaponData? item)
        {
            if (rightHandObject)
            {
                rightHandObject.SetActive(false);
                Destroy(rightHandObject);
                rightHandObject = null;
            }

            if (!item || !characterModelProvider)
            {
                anim.Play("Empty Right");
                return;
            }

            rightHandObject = Instantiate(item.weaponModel, characterModelProvider.GetRightHandPivot().transform);
            rightHandObject.transform.localPosition = Vector3.zero;
            rightHandObject.transform.localRotation = Quaternion.identity;
            EquipWeapon(item, false);
        }

        private void ChangeLeftHandItem(ItemWeaponData? item)
        {
            if (leftHandObject)
            {
                leftHandObject.SetActive(false);
                Destroy(leftHandObject);
                leftHandObject = null;
            }

            if (!item || !characterModelProvider)
            {
                anim.Play("Empty Left");
                return;
            }

            leftHandObject = Instantiate(item.weaponModel, characterModelProvider.GetLeftHandPivot().transform);
            leftHandObject.transform.localPosition = Vector3.zero;
            leftHandObject.transform.localRotation = Quaternion.identity;
            EquipWeapon(item, true);
        }

        private void EquipWeapon(ItemWeaponData w, bool isLeft = false)
        {
            if (!anim)
            {
                return;
            }

            string targetIdle = w.oh_idle_name;
            if (targetIdle.Length > 0)
            {
                targetIdle += isLeft ? "_l" : "_r";
            }
            else
            {
                if (isLeft)
                {
                    targetIdle = "Empty Left";
                }
                else
                {
                    targetIdle = "Empty Right";
                }
            }

            anim.SetBool("mirror", isLeft);
            anim.Play(targetIdle);
        }

        public void HandleTwoHanded(bool isTwoHanded, CharacterCreatorInventoryData? inventoryData)
        {
            anim.SetBool("twoHanded", isTwoHanded);

            if (inventoryData == null)
            {
                return;
            }

            bool isRight = true;
            ItemWeaponData currentWeapon = inventoryData.rightHandItem;
            if (currentWeapon == null)
            {
                isRight = false;
                currentWeapon = inventoryData.leftHandItem;
            }

            if (currentWeapon == null)
            {
                return;
            }

            if (isTwoHanded)
            {
                anim.Play(currentWeapon.th_idle_name);

                if (isRight)
                {
                    if (leftHandObject != null)
                    {
                        leftHandObject.SetActive(false);
                    }
                }
                else
                {
                    if (rightHandObject != null)
                    {
                        rightHandObject.SetActive(false);
                    }
                }
            }
            else
            {
                anim.Play("Empty Both");
                if (leftHandObject != null)
                {
                    leftHandObject.SetActive(true);
                }

                if (rightHandObject != null)
                {
                    rightHandObject.SetActive(true);
                }
            }
        }
        #endregion
    }

    [Serializable]
    public class CharacterCreatorInventoryData
    {
        public CharacterClass id;
        public ItemWeaponData rightHandItem;
        public ItemWeaponData leftHandItem;
        public bool isTwoHadned = false;
    }
}
