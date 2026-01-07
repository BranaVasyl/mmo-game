using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class QuestManager : MonoBehaviour
    {
        public List<Quest> quests;

        public static QuestManager singleton;
        void Awake()
        {
            singleton = this;
        }

        void Start()
        {
            ResetQustsStates();
        }

        public void SetActiveState(string questId, string stageId)
        {
            if (questId.Length == 0)
            {
                return;
            }

            Quest quest = GetQuest(questId);
            if (quest == null)
            {
                return;
            }

            if (quest.IsCompleted())
            {
                return;
            }

            QuestStageBase questStage = GetQuestStage(quest, stageId);
            if (!quest.IsActive() && !quest.IsCompleted())
            {
                ShowNotification("Квест розпочато", quest.questName, quest.questIcon, NotificationActionType.log);
                quest.OnStart();
            }
            if (questStage != null && !questStage.IsActive())
            {
                questStage.OnStart();
            }
        }

        public void SetCompleteState(string questId, string stageId)
        {
            if (questId.Length == 0)
            {
                return;
            }

            Quest quest = GetQuest(questId);
            if (quest == null)
            {
                return;
            }

            if (quest.IsCompleted())
            {
                return;
            }

            QuestStageBase questStage = GetQuestStage(quest, stageId);
            if (questStage == null)
            {
                if (!quest.IsCompleted())
                {
                    ShowNotification("Квест завершено", quest.questName, quest.questIcon, NotificationActionType.alert);
                    quest.OnComplete();
                }
            }
            else
            {
                if (!quest.IsActive() && !quest.IsCompleted())
                {
                    ShowNotification("Квест розпочато", quest.questName, quest.questIcon, NotificationActionType.log);
                    quest.OnStart();
                }

                if (!questStage.IsCompleted())
                {
                    ShowNotification("Квест обновлено", quest.questName, quest.questIcon, NotificationActionType.log);
                    questStage.OnComplete();
                }
            }
        }

        private Quest GetQuest(string id)
        {
            Quest resultQuest = quests.Find(i => i.id == id);
            return resultQuest;
        }

        private QuestStageBase GetQuestStage(Quest quest, string id)
        {
            QuestStageBase questStage = quest.questStages.Find(i => i.id == id);
            return questStage;
        }

        public void QuestOnActive(string separateString)
        {
            string[] command = separateString.Split(new char[] { '&' });

            if (command.Length == 0)
            {
                return;
            }

            string questId = command[0];
            string stageId = command.Length == 2 ? command[1] : "";

            SetActiveState(questId, stageId);
        }

        public void QuestOnCompleted(string separateString)
        {
            string[] command = separateString.Split(new char[] { '&' });

            if (command.Length == 0)
            {
                return;
            }

            string questId = command[0];
            string stageId = command.Length == 2 ? command[1] : "";

            SetCompleteState(questId, stageId);
        }

        public bool QuestIsActive(string separateString)
        {
            string[] command = separateString.Split(new char[] { '&' });

            Quest quest = command.Length >= 1 ? GetQuest(command[0]) : null;
            if (quest == null)
            {
                return false;
            }

            QuestStageBase questStage = command.Length == 2 ? GetQuestStage(quest, command[1]) : null;

            if (questStage != null)
            {
                return questStage.IsActive();
            }
            else
            {
                return quest.IsActive();
            }
        }

        public bool QuestIsCompleted(string separateString)
        {
            string[] command = separateString.Split(new char[] { '&' });

            Quest quest = command.Length >= 1 ? GetQuest(command[0]) : null;
            if (quest == null)
            {
                return false;
            }

            QuestStageBase questStage = command.Length == 2 ? GetQuestStage(quest, command[1]) : null;

            if (questStage != null)
            {
                return questStage.IsCompleted();
            }
            else
            {
                return quest.IsCompleted();
            }
        }

        private void ShowNotification(string title = "", string subtitle = "", Sprite icon = null, NotificationActionType type = NotificationActionType.log)
        {
            NotificationManager.singleton.AddNewNotification(new NotificationData(title, subtitle, icon, type));
        }

        private void ResetQustsStates()
        {
            for (int i = 0; i < quests.Count; i++)
            {
                Quest quest = quests[i];
                quest.active = false;
                quest.completed = false;

                for (int j = 0; j < quest.questStages.Count; j++)
                {
                    QuestStageBase questStage = quest.questStages[j];
                    questStage.active = false;
                    questStage.completed = false;
                }
            }
        }
    }
}
