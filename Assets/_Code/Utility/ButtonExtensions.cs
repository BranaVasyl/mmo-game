using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace Project.Utility
{
    public static class ButtonExtension
    {
        public static void AddEventListener<T>(this Button button, T param, Action<T> OnClick)
        {
            button.onClick.AddListener(delegate ()
            {
                OnClick(param);
            });
        }
    }
}