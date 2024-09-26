using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Networking;

namespace BV
{
    public class CharacterSelectorManager : MonoBehaviour
    {
        public GameObject selectorPanel;

        public void Init()
        {
            selectorPanel.SetActive(true);
        }

        public void Deinit()
        {
            selectorPanel.SetActive(false);
        }

        public void OnPlayClick()
        {
            //@todo pass character id
            NetworkClient.Instance.Emit("selectCharacter", null, (response) =>
            {
                if (SessionManager.Instance != null)
                {
                    SessionManager.Instance.characterData = new();
                }
            });
        }
    }
}
