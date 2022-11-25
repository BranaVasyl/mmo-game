using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

namespace BV
{
    public class NotificationManager : MonoBehaviour
    {
        public TMP_Text popupTitle;
        public TMP_Text popupSubtitle;
        public Image popupImage;

        [Header("Audio source")]
        public AudioClip allertAudio;

        public GameObject window;
        private CanvasGroup canvasGroup;
        private AudioSource audioSource;

        private Queue<NotificationData> popupQueue;
        private Coroutine queueChecker;

        private float currentTime = 0;
        private float showNotificationTime = 2f;

        public AnimationCurve notificationFadeCurve;

        public void Init()
        {
            showNotificationTime = notificationFadeCurve[notificationFadeCurve.length - 1].time;
            canvasGroup = window.GetComponent<CanvasGroup>();
            audioSource = GetComponent<AudioSource>();

            popupQueue = new Queue<NotificationData>();
            CleanPopupData();
        }

        public void AddNewNotification(NotificationData newData)
        {
            popupQueue.Enqueue(newData);
            if (queueChecker == null)
            {
                queueChecker = StartCoroutine(CheckQueue());
            }
        }

        private void ShowPopup(NotificationData data)
        {
            window.SetActive(true);
            popupTitle.text = data.title;
            popupSubtitle.text = data.subtitle;
            popupImage.sprite = data.icon;

            //@todo add more audio
            AudioClip targetAudio = allertAudio;
            switch (data.notificationAction)
            {
                case NotificationActionType.alert:
                    targetAudio = allertAudio;
                    break;
            }
            
            audioSource.PlayOneShot(targetAudio);

        }

        private IEnumerator CheckQueue()
        {
            do
            {
                ShowPopup(popupQueue.Dequeue());
                do
                {
                    currentTime += Time.deltaTime;
                    canvasGroup.alpha = notificationFadeCurve.Evaluate(currentTime);

                    yield return null;
                } while (currentTime < showNotificationTime);

                CleanPopupData();
                currentTime = 0;
            } while (popupQueue.Count > 0);

            window.SetActive(false);
            queueChecker = null;
        }

        private void CleanPopupData()
        {
            popupTitle.text = "";
            popupSubtitle.text = "";
            popupImage.sprite = null;
        }

        public static NotificationManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }

    public enum NotificationActionType
    {
        alert, error, complete
    }

    [Serializable]
    public class NotificationData
    {
        public string title;
        public string subtitle;
        public Sprite icon;
        public NotificationActionType notificationAction;

        public NotificationData(string t = "", string sT = "", Sprite i = null, NotificationActionType nA = NotificationActionType.alert)
        {
            title = t;
            subtitle = sT;
            icon = i;
            notificationAction = nA;
        }
    }
}
