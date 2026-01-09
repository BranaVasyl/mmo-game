using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BV
{
    public class WeatherManager : Singleton<WeatherManager>
    {
        [Range(0, 24)]
        public float timeOfDay = 0f;
        public float orbitSpeed = 1.0f;
        public float timeMultiplier = 1;

        [Header("Time")]
        public int hour = 0;
        public int minute = 0;

        private IWeatherScene currentScene;

        void Update()
        {
            UpdateTime();
        }

        public void RegisterScene(IWeatherScene scene)
        {
            currentScene = scene;
            ApplyToScene();
        }

        private void UpdateTime()
        {
            timeOfDay += Time.deltaTime * orbitSpeed * timeMultiplier;
            if (timeOfDay > 24)
            {
                timeOfDay = 0;
            }

            UpdateDisplayedTime();
            ApplyToScene();
        }

        void ApplyToScene()
        {
            if (currentScene == null)
            {
                 return;
            }

            currentScene.ApplyTime(timeOfDay);
        }

        public void SetWeatherState(float TOD, float OS, float TM)
        {
            timeOfDay = TOD;
            orbitSpeed = OS;
            timeMultiplier = TM;

            ApplyToScene();
        }

        void UpdateDisplayedTime()
        {
            TimeSpan temp = TimeSpan.FromHours(timeOfDay);
            hour = temp.Hours;
            minute = temp.Minutes;

            if (GameUIManager.Instance)
            {
                GameUIManager.Instance.currentTimeText.text =$"{hour:00} : {minute:00}";
            }
        }
    }
}
