using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BV
{
    public class CharacterCreatorManager : MonoBehaviour
    {
        [SerializeField] private Button playButton;

        public void OnPlayClick()
        {
            Debug.Log(1111111);
            SceneManager.LoadScene("SampleScene");
        }
    }
}
