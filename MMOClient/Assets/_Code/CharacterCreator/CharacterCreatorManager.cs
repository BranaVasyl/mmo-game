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

        public ItemWeaponData warriorR;
        public ItemWeaponData warriorL;

        public ItemWeaponData banditR;
        public ItemWeaponData banditL;

        public ItemWeaponData magicR;
        public ItemWeaponData magicL;


        private GameObject rightHandObject;

        private GameObject leftHandObject;


        private string slectedSex = "man";
        private string slectedRace = "human";
        private string slectedClass = "warrior";
        private GameObject currentCharacter;

        public GameObject creatorPanel;
        public Transform transformSpawnPoint;
        public Animator anim;


        private CharacterModelProvider characterModelProvider;

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

        private void ChangeItems()
        {
            if (slectedClass == "warrior")
            {
                ChangeRightHandItem(warriorR);
                ChangeLeftHandItem(warriorL);
            }
            else if (slectedClass == "magic")
            {
                ChangeRightHandItem(magicR);
                ChangeLeftHandItem(magicL);
            }
            else if (slectedClass == "bandit")
            {
                ChangeRightHandItem(banditR, true);
                ChangeLeftHandItem(banditL);
            }
        }

        private void ChangeRightHandItem(ItemWeaponData? item, bool twoHanded = false)
        {
            if (rightHandObject)
            {
                rightHandObject.SetActive(false);
                Destroy(rightHandObject);
                rightHandObject = null;
            }

            if (!item || !characterModelProvider)
            {
                return;
            }

            rightHandObject = Instantiate(item.weaponModel, characterModelProvider.GetRightHandPivot().transform);
            rightHandObject.transform.localPosition = Vector3.zero;
            rightHandObject.transform.localRotation = Quaternion.identity;
            EquipWeapon(item, false, twoHanded);
        }

        private void ChangeLeftHandItem(ItemWeaponData? item, bool twoHanded = false)
        {
            if (leftHandObject)
            {
                leftHandObject.SetActive(false);
                Destroy(leftHandObject);
                leftHandObject = null;
            }

            if (!item || !characterModelProvider)
            {
                return;
            }

            leftHandObject = Instantiate(item.weaponModel, characterModelProvider.GetLeftHandPivot().transform);
            leftHandObject.transform.localPosition = Vector3.zero;
            leftHandObject.transform.localRotation = Quaternion.identity;
            EquipWeapon(item, true, twoHanded);
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

        public void OnPlayClick()
        {
            SceneManager.LoadScene("SampleScene");
        }

        private void EquipWeapon(ItemWeaponData w, bool isLeft = false, bool twoHanded = false)
        {
            if (!anim)
            {
                return;
            }

            string targetIdle = twoHanded ? w.th_idle_name : w.oh_idle_name;
            if (!twoHanded)
            {
                anim.Play("Empty Both");

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
            }

            anim.SetBool("mirror", isLeft);
            anim.Play(targetIdle);
        }
    }
}
