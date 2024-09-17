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

        private string slectedSex = "man";
        private string slectedRace = "human";
        private string slectedClass = "warrior";
        private GameObject currentCharacter;

        public GameObject creatorPanel;
        public Transform transformSpawnPoint;

        public CharacterCreatorInventoryData[] characterCreatorInventoryDatas;
        private CharacterModelProvider characterModelProvider;
        private GameObject rightHandObject;
        private GameObject leftHandObject;

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

        public void OnSetClass(string characterClass)
        {
            if (slectedClass == characterClass)
            {
                return;
            }

            slectedClass = characterClass;
            ChangeItems();
        }

        public void CreateCharacter()
        {
            if (currentCharacter)
            {
                Destroy(currentCharacter);
            }

            string id = slectedSex.ToLower() + char.ToUpper(slectedRace[0]) + slectedRace.Substring(1).ToLower();
            currentCharacter = charactersController.CreateCharacter(id, transformSpawnPoint);

            if (!currentCharacter)
            {
                return;
            }

            anim = currentCharacter.GetComponent<Animator>();
            characterModelProvider = currentCharacter.GetComponent<CharacterModelProvider>();

            ChangeItems();
        }

        private void ChangeItems()
        {
            CharacterCreatorInventoryData? currentInventoryData = null;

            foreach (CharacterCreatorInventoryData inventoryData in characterCreatorInventoryDatas)
            {
                if (inventoryData.id.ToString() == slectedClass)
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

        public void OnPlayClick()
        {
            SceneManager.LoadScene("SampleScene");
        }

        #region change character items
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

    public enum CharacterClass
    {
        warrior,
        bandit,
        magic
    }
}
