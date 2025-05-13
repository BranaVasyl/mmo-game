using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace BV
{
    public class StepLider : MonoBehaviour
    {
        public Slider slider;
        public float currentValue = 0;
        public int maxValue = 0;
        public UnityEvent<float> onUpdateData = new UnityEvent<float>();

        void Start()
        {
            slider.onValueChanged.AddListener((v) =>
            {
                if (currentValue == v)
                {
                    return;
                }

                SetValue(v);
                OnUpdateValue(v);
            });

            SetValue(currentValue);
            SetMaxValue(maxValue);
        }

        public void SetMaxValue(int value = 0)
        {
            maxValue = value;
            slider.maxValue = value;
        }

        public void SetValue(float value = 0)
        {
            currentValue = value;
            slider.value = value;
        }

        public void OnPrevValue()
        {
            currentValue = Mathf.Ceil(currentValue);

            if (currentValue > 0)
            {
                currentValue -= 1;
            }

            SetValue(currentValue);
            OnUpdateValue(currentValue);
        }

        public void OnNextValue()
        {
            if (currentValue < maxValue)
            {
                currentValue = Mathf.Min(currentValue + 1, maxValue);
            }

            SetValue(Mathf.FloorToInt(currentValue));
            OnUpdateValue(currentValue);
        }

        public void OnUpdateValue(float value)
        {
            onUpdateData.Invoke(value);
        }
    }
}
