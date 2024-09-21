using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BV
{
    public class CharacterModelController : MonoBehaviour
    {
        public GameObject rightHandPivot;
        public GameObject leftHandPivot;

        public AvaibleCharacterCustomization avaibleCharacterCustomization;

        public GameObject GetRightHandPivot()
        {
            return rightHandPivot;
        }

        public GameObject GetLeftHandPivot()
        {
            return leftHandPivot;
        }

        public AvaibleCharacterCustomization GetAvaibleCharacterCustomization()
        {
            return avaibleCharacterCustomization;
        }

        public void UpdateCharacterCustomization(CharacterCustomizationData characterCustomizationData)
        {
            ApplyCharacterCustomization(characterCustomizationData);
        }

        public void SetCharacterCustomization(CharacterCustomizationData characterCustomizationData)
        {
            ApplyCharacterCustomization(characterCustomizationData, true);
        }

        private void ApplyCharacterCustomization(CharacterCustomizationData characterCustomizationData, bool removeAfterAply = false)
        {
            ApplyCharacterHairStyle(characterCustomizationData.hairId, removeAfterAply);
            ApplyCharacterHairColor(characterCustomizationData.hairColor);
        }

        private void ApplyCharacterHairStyle(string id, bool removeAfterAply = false)
        {
            GameObjectById[] hairList = avaibleCharacterCustomization.hairList;

            foreach (GameObjectById hair in hairList)
            {
                if (hair.gameObject == null)
                {
                    continue;
                }

                if (hair.id == id)
                {
                    hair.gameObject.SetActive(true);
                }
                else
                {
                    if (removeAfterAply)
                    {
                        Destroy(hair.gameObject);
                    }
                    else
                    {
                        hair.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void ApplyCharacterHairColor(string color)
        {
            Debug.Log(color);
        }
    }

    [Serializable]
    public class CharacterCustomizationData
    {
        public string hairId;
        public string hairColor;
    }

    [Serializable]
    public class AvaibleCharacterCustomization
    {
        public GameObjectById[] hairList;
        public Color[] hairCollorPallete;
    }
}
