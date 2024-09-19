using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BV
{
    public class CharactersController : MonoBehaviour
    {
        public CharacterModelData[] characters;

        public GameObject CreateCharacter(CharacterData characterData, Transform transform, bool isStatic = true)
        {
            GameObject newCharacter = null;

            string id = characterData.gender.ToLower() + char.ToUpper(characterData.race[0]) + characterData.race.Substring(1).ToLower();
            foreach (CharacterModelData modelData in characters)
            {
                if (modelData.id == id)
                {
                    newCharacter = Instantiate(modelData.gameObject, transform);
                    newCharacter.transform.position = transform.position;
                    break;
                }
            }

            if (newCharacter != null)
            {
                CharacterModelController сharacterModelController = newCharacter.GetComponent<CharacterModelController>();
                if (сharacterModelController != null)
                {
                    if (isStatic)
                    {
                        сharacterModelController.SetCharacterCustomization(characterData.characterCustomizationData);
                    }
                    else
                    {
                        сharacterModelController.UpdateCharacterCustomization(characterData.characterCustomizationData);
                    }
                }
            }

            return newCharacter;
        }
    }

    [Serializable]
    public class CharacterData
    {
        public string gender;
        public string race;
        public string characterClass;
        public string alliance;
        public CharacterCustomizationData characterCustomizationData;

        public CharacterData()
        {
            gender = "man";
            race = "human";
            characterClass = "warrior";
            alliance = "alliance1";
            characterCustomizationData = new CharacterCustomizationData();
        }
    }

    [Serializable]
    public enum Gender
    {
        man,
        woman
    }

    [Serializable]
    public enum Race
    {
        human,
        elf,
        dwarf
    }

    [Serializable]
    public enum CharacterClass
    {
        warrior,
        bandit,
        magic
    }
}
