using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/Character", order = 3)]
    public class CharacterData : ScriptableObject
    {
        [HideInInspector]
        public string id;

        public Gender sex;
        public Race race;
        public GameObject gameObject;

         private void GenerateID()
        {
            id = sex.ToString().ToLower() + char.ToUpper(race.ToString()[0]) + race.ToString().Substring(1).ToLower();
        }

        private void OnValidate()
        {
            GenerateID();
        }
    }

    public enum Gender
    {
        man,
        woman
    }

    public enum Race
    {
        human,
        elf,
        dwarf
    }
}
