using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BV
{
    public class PieMenuItem : MonoBehaviour
    {
        public Color hoverColor;
        public Color baseColor;
        public Image icon;
        public Image background;

        void Start()
        {
            background.color = baseColor;
        }

        public void Select()
        {
            background.color = hoverColor;
        }

        public void Deselect()
        {
            background.color = baseColor;
        }
    }
}