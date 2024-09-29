using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BV
{
    public class CharactersController : Singleton<CharactersController>
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

            if (newCharacter != null && isStatic)
            {
                CharacterModelController сharacterModelController = newCharacter.GetComponent<CharacterModelController>();
                if (сharacterModelController != null)
                {
                    сharacterModelController.SetCharacterCustomization(characterData.customization);
                }
            }

            return newCharacter;
        }
    }

    [Serializable]
    public class CharacterData
    {
        public string id;
        public string name;
        public string gender;
        public string race;
        public string characterClass;
        public string alliance;
        public CharacterCustomizationData customization;

        public CharacterData()
        {
            id = null;
            name = "";
            gender = "man";
            race = "human";
            characterClass = "warrior";
            alliance = "alliance1";
            customization = new CharacterCustomizationData();
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
