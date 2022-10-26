using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class WeatherManager : MonoBehaviour
    {
        public int hour = 0;
        public int minute = 0;

        private float currentTime = 0f;

        public void Init()
        {

        }

        public void Update()
        {
            currentTime += Time.deltaTime;
            if ((int)currentTime == minute)
            {
                return;
            }

            if (currentTime >= 60)
            {
                if (hour == 23)
                {
                    hour = 0;
                }
                else
                {
                    hour++;
                }

                currentTime = 0;
            }
            minute = (int)currentTime;

            UpdateDisplayedTime();
        }

        private void UpdateDisplayedTime()
        {
            string displayHour = hour < 10 ? "0" + hour : hour.ToString();
            string dispayMinute = minute < 10 ? "0" + minute : minute.ToString();

            GameUIManager.singleton.currentTimeText.text = string.Format("{0} : {1}", displayHour, dispayMinute);
        }

        public static WeatherManager singleton;
        void Awake()
        {
            singleton = this;
        }

    }
}
