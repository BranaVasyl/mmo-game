using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class CharactersController : MonoBehaviour
    {
        public CharacterData[] characters;


        public GameObject CreateCharacter(string id, Transform transform)
        {
            foreach (CharacterData characterData in characters)
            {
                if (characterData.id == id)
                {
                    GameObject newCharacter = Instantiate(characterData.gameObject, transform);
                    newCharacter.transform.position = transform.position;

                    return newCharacter;
                }
            }

            return null;
        }
    }
}
