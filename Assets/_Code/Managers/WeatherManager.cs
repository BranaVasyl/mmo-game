using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BV
{
    public class WeatherManager : MonoBehaviour
    {
        [Range(0, 24)]
        public float timeOfDay = 0f;

        [HideInInspector]
        public float orbitSpeed = 1.0f;
        [HideInInspector]
        public float timeMultiplier = 1;

        [Header("Pretty Time")]
        public int hour = 0;
        public int minute = 0;

        [Header("Lighting")]
        public Light sun;
        public Light moon;

        [Header("Sky")]
        public Volume skyVolume;
        public AnimationCurve starsCurve;

        private bool isNight;
        private PhysicallyBasedSky sky;

        public void Init()
        {
            skyVolume.profile.TryGet(out sky);
        }

        private void OnValidate()
        {
            skyVolume.profile.TryGet(out sky);
            UpdateSunPosition();
        }

        public void Update()
        {
            UpdateTime();
        }

        public void UpdateWeatherData(float tOD, float oS, float tM)
        {
            UpdateTime(tOD);
            orbitSpeed = oS;
            timeMultiplier = tM;
        }

        public void SetOrbitSbeed(float oS) {
            orbitSpeed = oS;
        }

        private void UpdateTime(float time = -1f)
        {
            if (time == -1)
            {
                timeOfDay += Time.deltaTime * orbitSpeed * timeMultiplier;
            }
            else
            {
                timeOfDay = time;
            }

            if (timeOfDay > 24)
            {
                timeOfDay = 0;
            }

            UpdateSunPosition();
            if (TimeSpan.FromHours(timeOfDay).Minutes != minute)
            {
                UpdateDisplayedTime();
            }
        }

        private void UpdateSunPosition()
        {
            float alpha = timeOfDay / 24.0f;
            float sunRotation = Mathf.Lerp(-90, 270, alpha);
            float moonRotation = sunRotation - 180;

            sun.transform.rotation = Quaternion.Euler(sunRotation, -150.0f, 0);
            moon.transform.rotation = Quaternion.Euler(moonRotation, -150.0f, 0);

            sky.spaceEmissionMultiplier.value = starsCurve.Evaluate(alpha) * 27000.0f;
            sky.spaceRotation.value = new Vector3(sunRotation, -150.0f, 0);

            CheckNightDayTransition();
        }

        private void CheckNightDayTransition()
        {
            if (isNight)
            {
                if (moon.transform.rotation.eulerAngles.x > 180)
                {
                    StartDay();
                }
            }
            else
            {
                if (sun.transform.rotation.eulerAngles.x > 180)
                {
                    StartNight();
                }
            }
        }

        private void StartDay()
        {
            isNight = false;
            sun.shadows = LightShadows.Soft;
            moon.shadows = LightShadows.None;
        }

        private void StartNight()
        {
            isNight = true;
            sun.shadows = LightShadows.None;
            moon.shadows = LightShadows.Soft;
        }

        private void UpdateDisplayedTime()
        {
            TimeSpan temp = TimeSpan.FromHours(timeOfDay);
            hour = temp.Hours;
            minute = temp.Minutes;

            string displayHour = hour < 10 ? "0" + hour : hour.ToString();
            string dispayMinute = minute < 10 ? "0" + minute : minute.ToString();

            if (GameUIManager.singleton)
            {
                GameUIManager.singleton.currentTimeText.text = string.Format("{0} : {1}", displayHour, dispayMinute);
            }
        }

        public static WeatherManager singleton;
        void Awake()
        {
            singleton = this;
        }

    }
}
