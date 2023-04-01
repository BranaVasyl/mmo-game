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
        [Header("Notification Data")]
        public GameObject notificationTemplate;
        public TMP_Text notificationTitle;
        public TMP_Text notificationSubtitle;
        public Image notificationImage;
        public AnimationCurve notificationFadeCurve;

        private Queue<NotificationData> notificationQueue;
        private Coroutine notificationQueueChecker;
        private CanvasGroup canvasGroup;
        private float currentNotificationTime = 0;
        private float showNotificationTime = 2f;

        [Header("Message Data")]
        public GameObject messageContainer;
        public GameObject messageTemplate;
        public AnimationCurve messageFadeCurve;

        private List<MessageNotificationData> messagesDataQueue = new List<MessageNotificationData>();
        private Queue<string> messagesQueue;
        private Coroutine messagesQueueChecker;
        private float showMessageTime = 2f;

        [Header("Audio Source")]
        public AudioClip allertAudio;
        public AudioClip logAudio;
        private AudioSource audioSource;

        public void Init()
        {
            //notification
            notificationTemplate.SetActive(false);
            showNotificationTime = notificationFadeCurve[notificationFadeCurve.length - 1].time;
            canvasGroup = notificationTemplate.GetComponent<CanvasGroup>();
            audioSource = GetComponent<AudioSource>();

            notificationQueue = new Queue<NotificationData>();
            CleanNotificationData();

            //message
            messageContainer.SetActive(true);
            messageTemplate.SetActive(false);
            showMessageTime = messageFadeCurve[messageFadeCurve.length - 1].time;
            messagesQueue = new Queue<string>();
        }

        #region Notification
        public void AddNewNotification(NotificationData newData)
        {
            notificationQueue.Enqueue(newData);
            if (notificationQueueChecker == null)
            {
                notificationQueueChecker = StartCoroutine(CheckNotificationQueue());
            }
        }

        private void ShowNotification(NotificationData data)
        {
            notificationTemplate.SetActive(true);
            notificationTitle.text = data.title;
            notificationSubtitle.text = data.subtitle;
            notificationImage.sprite = data.icon;

            //@todo add more audio
            AudioClip targetAudio = logAudio;
            switch (data.notificationAction)
            {
                case NotificationActionType.log:
                    targetAudio = logAudio;
                    break;
                case NotificationActionType.alert:
                    targetAudio = allertAudio;
                    break;
            }

            audioSource.PlayOneShot(targetAudio);
        }

        private IEnumerator CheckNotificationQueue()
        {
            do
            {
                ShowNotification(notificationQueue.Dequeue());
                do
                {
                    currentNotificationTime += Time.deltaTime;
                    canvasGroup.alpha = notificationFadeCurve.Evaluate(currentNotificationTime);

                    yield return null;
                } while (currentNotificationTime < showNotificationTime);

                CleanNotificationData();
                currentNotificationTime = 0;
            } while (notificationQueue.Count > 0);

            notificationTemplate.SetActive(false);
            notificationQueueChecker = null;
        }

        private void CleanNotificationData()
        {
            notificationTitle.text = "";
            notificationSubtitle.text = "";
            notificationImage.sprite = null;
        }
        #endregion

        #region Message
        public void AddNewMessage(string message)
        {
            messagesQueue.Enqueue(message);
            if (messagesQueueChecker == null)
            {
                messagesQueueChecker = StartCoroutine(CheckMessagesQueue());
            }
        }

        private MessageNotificationData ShowMessage(string message)
        {
            GameObject g = Instantiate(messageTemplate, messageContainer.transform);
            g.SetActive(true);
            g.GetComponent<TMP_Text>().text = message;

            return new MessageNotificationData(message, g);
        }

        private IEnumerator CheckMessagesQueue()
        {
            do
            {

                while (messagesDataQueue.Count < 5 && messagesQueue.Count > 0)
                {
                    messagesDataQueue.Add(ShowMessage(messagesQueue.Dequeue()));
                }

                for (int i = 0; i < messagesDataQueue.Count; i++)
                {
                    MessageNotificationData messageData = messagesDataQueue[i];
                    if (messageData.timer >= showMessageTime)
                    {
                        Destroy(messageData.messageTemplate);
                        messagesDataQueue.Remove(messageData);
                        continue;
                    }

                    messageData.timer += Time.deltaTime;
                    messageData.messageTemplate.GetComponent<CanvasGroup>().alpha = messageFadeCurve.Evaluate(messageData.timer);
                }

                if (messagesDataQueue.Count > 0)
                {
                    yield return null;
                }
            } while (messagesQueue.Count > 0 || messagesDataQueue.Count > 0);

            messagesQueueChecker = null;
        }
        #endregion

        public static NotificationManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }

    public enum NotificationActionType
    {
        log, alert, error, complete
    }

    [Serializable]
    public class NotificationData
    {
        public string title;
        public string subtitle;
        public Sprite icon;
        public NotificationActionType notificationAction;

        public NotificationData(string t = "", string sT = "", Sprite i = null, NotificationActionType nA = NotificationActionType.log)
        {
            title = t;
            subtitle = sT;
            icon = i;
            notificationAction = nA;
        }
    }

    [Serializable]
    public class MessageNotificationData
    {
        public string message;
        public GameObject messageTemplate;
        public float timer;

        public MessageNotificationData(string m = "", GameObject mT = null, float t = 0)
        {
            message = m;
            messageTemplate = mT;
            timer = t;
        }
    }
}
