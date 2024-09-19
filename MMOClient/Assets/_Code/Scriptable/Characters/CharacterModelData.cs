using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "CharacterModelData", menuName = "Scriptable Objects/Character", order = 3)]
    public class CharacterModelData : ScriptableObject
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
}
