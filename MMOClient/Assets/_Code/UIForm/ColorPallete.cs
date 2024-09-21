using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace BV
{
    public class ColorPallete : MonoBehaviour
    {
        public GameObject palleteContainer;
        public GameObject palleteFrame;
        public Color[] colorList;

        private List<GameObject> paletteItems = new List<GameObject>();
        public UnityEvent<Color, int> onUpdateData = new UnityEvent<Color, int>();

        void Start()
        {
            SetPalette(colorList);
        }

        public void SetPalette(Color[] colors)
        {
            CleanPalette();

            colorList = colors;
            GeneratePallete();
        }

        private void CleanPalette()
        {
            for (int i = 0; i < paletteItems.Count; i++)
            {
                Destroy(paletteItems[i]);
            }

            colorList = new Color[0];
        }

        private void GeneratePallete()
        {
            GameObject g;
            for (int i = 0; i < colorList.Length; i++)
            {
                int index = i;
                g = Instantiate(palleteFrame, palleteContainer.transform);

                GameObject buttonContainer = g.transform.GetChild(0).gameObject;
                buttonContainer.GetComponent<Image>().color = colorList[index];
                buttonContainer.GetComponent<Button>().onClick.AddListener(() => OnChangeColor(colorList[index], index));

                g.SetActive(true);
                paletteItems.Add(g);
            }
        }

        public void OnChangeColor(Color color, int index)
        {
            onUpdateData.Invoke(color, index);
        }
    }
}

