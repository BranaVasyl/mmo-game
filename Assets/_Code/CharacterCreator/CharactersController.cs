using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BV
{
    public class CharactersController : Singleton<CharactersController>
    {
        public CharacterModelData[] characters;

        public GameObject CreateCharacter(PlayerData playerData, Transform transform, bool isStatic = true)
        {
            GameObject newCharacter = null;

            string id = playerData.gender.ToLower() + char.ToUpper(playerData.race[0]) + playerData.race.Substring(1).ToLower();
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
                    сharacterModelController.SetCharacterCustomization(playerData.customization);
                }
            }

            return newCharacter;
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
