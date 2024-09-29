using System.Collections;
using System.Collections.Generic;
using Project.Utility;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace BV
{
    public class ApplicationManager : Singleton<ApplicationManager>
    {
        [Header("Loadeded Screen")]
        [SerializeField]
        private Transform _loadedScreen;

        [Header("Modal Screen")]
        [SerializeField]
        private Transform _modalScreen;

        [Header("Header")]
        [SerializeField]
        private Transform _headerArea;
        [SerializeField]
        private TextMeshProUGUI _titleField;

        [Header("Content")]
        [SerializeField]
        private Transform _contentArea;
        [SerializeField]
        private TextMeshProUGUI _contentField;

        [Header("Footer")]
        [SerializeField]
        private Transform _footerArea;
        [SerializeField]
        private Button _confirmButton;
        [SerializeField]
        private Button _declineButton;

        private System.Action onConfirmAction;
        private System.Action onDeclineAction;

        [Header("Spiner Screen")]
        [SerializeField]
        private Transform _spinerScreen;

        void Start()
        {
            ShowLoadingScreen();
            CloseModal();
            CloseSpinerLoader();
            SceneManagementManager.Instance.LoadLevel(SceneList.LOGIN_SCENE, (levelName) => { }, true);
        }

        #region loadingScreen
        public void ShowLoadingScreen()
        {
            _loadedScreen.gameObject.SetActive(true);
        }

        public void HideLoadingScreen()
        {
            _loadedScreen.gameObject.SetActive(false);
        }
        #endregion

        #region modalScreen
        public void ShowInformationModal(string message = null)
        {
            ShowModal(null, message);
        }

        public void ShowConfirmationModal(string message = null, System.Action confirmAction = null)
        {
            ShowModal(null, message, () =>
            {
                confirmAction?.Invoke();
                CloseModal();
            });
        }

        public void ShowModal(string title = null, string message = null, System.Action confirmAction = null, System.Action declineAction = null)
        {
            _modalScreen.gameObject.SetActive(true);

            bool hasTitle = string.IsNullOrEmpty(title);
            _headerArea.gameObject.SetActive(!hasTitle);
            _titleField.text = title;

            _contentField.text = message;

            _footerArea.gameObject.SetActive(false);
            _confirmButton.gameObject.SetActive(false);
            _confirmButton.gameObject.SetActive(false);

            if (confirmAction != null || declineAction != null)
            {
                _footerArea.gameObject.SetActive(true);

                if (confirmAction != null)
                {
                    onConfirmAction = confirmAction;
                    _confirmButton.gameObject.SetActive(true);
                }

                if (declineAction != null)
                {
                    onDeclineAction = declineAction;
                    _confirmButton.gameObject.SetActive(true);
                }
            }
        }

        public void CloseModal()
        {
            onConfirmAction = null;
            onDeclineAction = null;
            _modalScreen.gameObject.SetActive(false);
        }

        public void OnConfirmModal()
        {
            onConfirmAction?.Invoke();
        }

        public void OnDeclineModal()
        {
            onDeclineAction?.Invoke();
        }
        #endregion

        #region SpinerLoaderScreen
        public void ShowSpinerLoader()
        {
            _spinerScreen.gameObject.SetActive(true);
        }

        public void CloseSpinerLoader()
        {
            _spinerScreen.gameObject.SetActive(false);
        }
        #endregion
    }
}

