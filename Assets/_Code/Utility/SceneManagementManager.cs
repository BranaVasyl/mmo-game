using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

namespace BV
{
    public class SceneManagementManager : Singleton<SceneManagementManager>
    {
        private List<LevelLoadingData> levelsLoading;
        private List<string> currentlyLoadedScenes;

        private GameUIManager gameUIManager;

        void Start()
        {
            gameUIManager = GameUIManager.Instance;
        }

        public override void Awake()
        {
            base.Awake();
            levelsLoading = new List<LevelLoadingData>();
            currentlyLoadedScenes = new List<string>();
        }

        public void Update()
        {
            for (int i = levelsLoading.Count - 1; i >= 0; i--)
            {
                if (levelsLoading[i] == null)
                {
                    levelsLoading.RemoveAt(i);
                    continue;
                }

                if (levelsLoading[i].ao.isDone)
                {
                    levelsLoading[i].ao.allowSceneActivation = true;
                    currentlyLoadedScenes.Add(levelsLoading[i].sceneName);

                    levelsLoading[i].onLevelLoaded.Invoke(levelsLoading[i].sceneName);
                    levelsLoading.RemoveAt(i);
                }
            }
        }

        public void ReplaceLevel(string levelName, Action<string> onLevelLoaded, bool isShowingLoadingScreen = false)
        {
            var nonStaticLoadedScenes = SceneList.GetNonStaticScenes().Where(scene => currentlyLoadedScenes.Contains(scene)).ToList();
            foreach (string scene in nonStaticLoadedScenes)
            {
                UnloadLevel(scene);
            }

            LoadLevel(
                levelName,
                (loadedLevelName) => { onLevelLoaded(loadedLevelName); },
                isShowingLoadingScreen
            );
        }

        public void LoadLevel(string levelName, Action<string> onLevelLoaded, bool isShowingLoadingScreen = false)
        {
            bool value = currentlyLoadedScenes.Any(x => x == levelName);

            if (value)
            {
                Debug.LogFormat("Current level ({0}) is already loaded into the game.", levelName);
            }

            LevelLoadingData lld = new LevelLoadingData();
            lld.ao = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
            lld.sceneName = levelName;
            lld.onLevelLoaded = (levelName) => {
                ApplicationManager.Instance.HideLoadingScreen();
                ApplySceneSettings();

                onLevelLoaded(levelName);
            };
            levelsLoading.Add(lld);

            if (isShowingLoadingScreen)
            {
                ApplicationManager.Instance.ShowLoadingScreen();
            }
        }

        public void UnloadLevel(string levelName)
        {
            foreach (string item in currentlyLoadedScenes)
            {
                if (item == levelName)
                {
                    SceneManager.UnloadSceneAsync(levelName);
                    currentlyLoadedScenes.Remove(item);
                    return;
                }
            }

            Debug.LogErrorFormat("Failed to unload level ({0}), most likely was never loaded to begin with or was already unloaded", levelName);
        }

        private void ApplySceneSettings()
        {
            if (gameUIManager == null)
            {
                return;
            }

            //@todo move to scripatbleObject config
            if (currentlyLoadedScenes.Any(x => x == SceneList.SAMPLE_SCENE || x == SceneList.FIRST_DANGE))
            {
                gameUIManager.ShowNotificationUI();
                gameUIManager.ShowWeatherUI();
                gameUIManager.ShowChatUI();
            } else {

                gameUIManager.HideNotificationUI();
                gameUIManager.HideWeatherUI();
                gameUIManager.HideChatUI();
            }
        }
    }

    [Serializable]
    public class LevelLoadingData
    {
        public AsyncOperation ao;
        public string sceneName;
        public Action<string> onLevelLoaded;
    }

    public static class SceneList
    {
        public const string INTRO = "Intro";
        public const string LOGIN_SCENE = "LoginScene";
        public const string CHARACTER_CREATOR_SCENE = "CharacterCreatorScene";
        public const string SAMPLE_SCENE = "SampleScene";
        public const string FIRST_DANGE = "FirstDange";
        public const string ONLINE = "Online";

        public static readonly string[] staticScenes = { INTRO, ONLINE };
        public static readonly string[] AllScenes = { INTRO, LOGIN_SCENE, CHARACTER_CREATOR_SCENE, FIRST_DANGE, SAMPLE_SCENE, ONLINE };

        public static string[] GetNonStaticScenes()
        {
            return AllScenes.Except(staticScenes).ToArray();
        }

        public static readonly Dictionary<string, string> sceneMapping = new Dictionary<string, string>
        {
            { "CHARACTER_CREATOR_SCENE", CHARACTER_CREATOR_SCENE },
            { "FIRST_DANGE", FIRST_DANGE },
            { "INTRO", INTRO },
            { "LOGIN_SCENE", LOGIN_SCENE },
            { "SAMPLE_SCENE", SAMPLE_SCENE },
            { "ONLINE", ONLINE }
        };
    }
}
