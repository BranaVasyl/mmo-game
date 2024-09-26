using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class CharacterCreatorSceneManager : MonoBehaviour
    {
        public CharacterCreatorManager characterCreatorManager;
        public CharacterSelectorManager characterSelectorManager;

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
            characterCreatorManager.Init(this);
            characterSelectorManager.Deinit();
        }
    }
}
