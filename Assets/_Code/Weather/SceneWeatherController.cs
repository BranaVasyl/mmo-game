using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BV
{
    public class SceneWeatherController : MonoBehaviour, IWeatherScene
    {
        [Header("Lighting")]
        [SerializeField] Light sun;
        [SerializeField] Light moon;

        [Header("Sky")]
        [SerializeField] Volume skyVolume;
        [SerializeField] AnimationCurve starsCurve;

        private PhysicallyBasedSky sky;
        private bool isNight;

        private WeatherManager weatherManager;

        void Start()
        {
            skyVolume.profile.TryGet(out sky);

            weatherManager = WeatherManager.Instance;
            weatherManager.RegisterScene(this);
        }

        public void ApplyTime(float timeOfDay)
        {
            float alpha = timeOfDay / 24f;
            float sunRotation = Mathf.Lerp(-90, 270, alpha);
            float moonRotation = sunRotation - 180;

            sun.transform.rotation = Quaternion.Euler(sunRotation, -150, 0);
            moon.transform.rotation = Quaternion.Euler(moonRotation, -150, 0);

            sky.spaceEmissionMultiplier.value = starsCurve.Evaluate(alpha) * 27000f;
            sky.spaceRotation.value = new Vector3(sunRotation, -150, 0);

            CheckNightDay();
        }

        void CheckNightDay()
        {
            if (!isNight && sun.transform.eulerAngles.x > 180)
            {
                StartNight();
            }
            else if (isNight && moon.transform.eulerAngles.x > 180)
            {
                StartDay();
            }
        }

        void StartDay()
        {
            isNight = false;
            sun.shadows = LightShadows.Soft;
            moon.shadows = LightShadows.None;
        }

        void StartNight()
        {
            isNight = true;
            sun.shadows = LightShadows.None;
            moon.shadows = LightShadows.Soft;
        }

        void OnDisable()
        {
            if (weatherManager != null)
            {
                weatherManager.RegisterScene(null);
            }
        }
    }
}