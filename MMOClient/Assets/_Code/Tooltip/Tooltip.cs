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
        public LayoutElement layoutElement;
        public AnimationCurve tooltipFadeCurve;

        [Header("Header Section")]
        public GameObject headerContainer;
        public TextMeshProUGUI titleField;
        public TextMeshProUGUI subtitleField;

        [Header("Content Section")]
        public GameObject contentContainer;
        public TextMeshProUGUI contentField;

        [Header("Footer Section")]
        public GameObject footerContainer;
        public GameObject massContainer;
        public TextMeshProUGUI massField;
        public GameObject priceContainer;
        public TextMeshProUGUI priceField;

        private CanvasGroup canvasGroup;
        private NewPlayerControls inputActions;
        private TooltipData tooltipData;
        private RectTransform rectTransform;

        private float showTooltiTime = 0;
        private float tooltipTimer = 0;

        public void Init()
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

        private void Update()
        {
            if (tooltipData == null)
            {
                return;
            }

            if (tooltipTimer < showTooltiTime)
            {
                tooltipTimer += Time.deltaTime;
                canvasGroup.alpha = tooltipFadeCurve.Evaluate(tooltipTimer);
            }

            if (tooltipData.position == Vector2.zero)
            {
                SetPosition(inputActions.Mouse.MousePosition.ReadValue<Vector2>());
            }
        }

        public void SetData(TooltipData data)
        {
            tooltipData = data;
            bool showHeader = true;
            bool showContent = !string.IsNullOrEmpty(data.content);
            bool showFooter = !string.IsNullOrEmpty(data.mass) || !string.IsNullOrEmpty(data.price);
            bool flexWidth = !showContent && !showFooter && (data.flexWidth || string.IsNullOrEmpty(data.title));

            // renderHeader
            if (showHeader)
            {
                if (!string.IsNullOrEmpty(data.title))
                {
                    titleField.text = data.title;
                    titleField.gameObject.SetActive(true);
                }

                if (!string.IsNullOrEmpty(data.subtitle))
                {
                    subtitleField.text = data.subtitle;
                    subtitleField.gameObject.SetActive(true);
                }

                headerContainer.SetActive(true);
            }

            // renderContent
            if (showContent)
            {
                if (!string.IsNullOrEmpty(data.content))
                {
                    contentField.text = data.content;
                    contentField.gameObject.SetActive(true);
                }

                contentContainer.SetActive(true);
            }

            // renderFooter
            if (showFooter)
            {
                if (!string.IsNullOrEmpty(data.mass))
                {
                    massField.text = data.mass;
                    massContainer.SetActive(true);
                }

                if (string.IsNullOrEmpty(data.price))
                {
                    priceField.text = data.price;
                    priceContainer.SetActive(true);
                }

                footerContainer.SetActive(true);
            }

            //auto width only header
            if (flexWidth)
            {
                layoutElement.enabled = Math.Max(titleField.preferredWidth, subtitleField.preferredWidth) >= layoutElement.preferredWidth;
            }
            else
            {
                layoutElement.enabled = true;
            }

            SetPosition(tooltipData.position);
        }

        private void SetPosition(Vector2 position)
        {
            float pivotX = 0;
            float pivotY = 0;

            bool top = Screen.height / 2 <= position.y;
            bool right = Screen.width / 2 <= position.x;
            if (right)
            {
                if (top)
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
            else
            {
                if (top)
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

            rectTransform.pivot = new Vector2(pivotX, pivotY);
            transform.position = position;
        }

        public void Clean()
        {
            headerContainer.SetActive(false);
            titleField.gameObject.SetActive(false);
            subtitleField.gameObject.SetActive(false);

            contentContainer.SetActive(false);
            contentField.gameObject.SetActive(false);

            footerContainer.SetActive(false);
            massContainer.SetActive(false);
            priceContainer.SetActive(false);

            titleField.text = "";
            subtitleField.text = "";
            contentField.text = "";
            massField.text = "";
            priceField.text = "";

            tooltipTimer = 0;
            tooltipData = null;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }
        }
    }
}
