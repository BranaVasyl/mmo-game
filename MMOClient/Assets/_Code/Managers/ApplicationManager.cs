using System.Collections;
using System.Collections.Generic;
using Project.Utility;
using UnityEngine;

namespace BV
{
    public class ApplicationManager : MonoBehaviour
    {
        void Start()
        {
            SceneManagementManager.Instance.LoadLevel(SceneList.LOGIN_SCENE, (levelName) => { });
        }
    }
}

