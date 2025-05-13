using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BV
{
    public class CharacterCreatorSceneManager : MonoBehaviour
    {
        public CharacterCreatorManager characterCreatorManager;
        public CharacterSelectorManager characterSelectorManager;

        public static CharacterCreatorSceneManager singleton;
        private void Awake()
        {
            singleton = this;
        }

        void Start()
        {
            OnGoSelectCharacterMode();
        }

        public void OnGoSelectCharacterMode()
        {
            characterCreatorManager.Deinit();
            characterSelectorManager.Init();
        }

        public void OnGoCreateCharacterMode()
        {
            characterCreatorManager.Init();
            characterSelectorManager.Deinit();
        }
    }
}
