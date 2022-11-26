using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

namespace BV
{
    [ExecuteInEditMode()]
    public class Tooltip : MonoBehaviour
    {
        public TextMeshProUGUI titleField;
        public TextMeshProUGUI subtitleField;
        public LayoutElement layoutElement;
        public AnimationCurve tooltipFadeCurve;

        private CanvasGroup canvasGroup;
        private NewPlayerControls inputActions;
        private TooltipData tooltipData;
        private RectTransform rectTransform;

        private float showTooltiTime = 0;
        private float tooltipTimer = 0;

        private void Start()
        {
            if (inputActions == null)
            {
                inputActions = new NewPlayerControls();
            }

            inputActions.Enable();

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;

            rectTransform = GetComponent<RectTransform>();
            showTooltiTime = tooltipFadeCurve[tooltipFadeCurve.length - 1].time;
        }

        public void SetData(TooltipData data)
        {
            tooltipData = data;
            if (string.IsNullOrEmpty(data.title))
            {
                titleField.gameObject.SetActive(false);
            }
            else
            {
                titleField.gameObject.SetActive(true);
                titleField.text = data.title;
            }

            subtitleField.text = data.subtitle;


            if (false)
            {
                //auto width tooltip
                layoutElement.enabled = Math.Max(titleField.preferredWidth, subtitleField.preferredWidth) >= layoutElement.preferredWidth;
            }
            else
            {
                layoutElement.enabled = true;
            }
        }

        private void Update()
        {
            if (tooltipData == null)
            {
                return;
            }

            tooltipTimer += Time.deltaTime;
            if (tooltipTimer < showTooltiTime)
            {
                canvasGroup.alpha = tooltipFadeCurve.Evaluate(tooltipTimer);
            }

            Vector2 position = inputActions.Mouse.MousePosition.ReadValue<Vector2>();

            float pivotX = 0;
            float pivotY = 0;

            bool left = Screen.width / 2 >= position.x;
            bool bottom = Screen.height / 2 >= position.y;
            if (left)
            {
                if (!bottom)
                {
                    pivotX = 0;
                    pivotY = 1;
                }
                else
                {
                    pivotX = 0;
                    pivotY = 0;
                }
            }
            else
            {
                if (!bottom)
                {
                    pivotX = 1;
                    pivotY = 1;
                }
                else
                {
                    pivotX = 1;
                    pivotY = 0;
                }
            }

            rectTransform.pivot = new Vector2(pivotX, pivotY);
            transform.position = position;
        }

        public void Clean()
        {
            titleField.text = "";
            subtitleField.text = "";
            tooltipTimer = 0;
            tooltipData = null;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }
        }
    }

    [Serializable]
    public class TooltipData
    {
        public string title;
        public string subtitle;

        public TooltipData(string t = "", string sT = "")
        {
            title = t;
            subtitle = sT;
        }
    }
}
