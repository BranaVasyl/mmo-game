using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Project.Utility;

namespace BV
{
    public class JournalController : MenuPanel
    {
        [SerializeField]
        private GameObject questListContainer;
        [SerializeField]
        private GameObject questStageList;
        public Sprite emptyMark;
        public Sprite correctMark;
        public Sprite incorrectMark;
        public Sprite targetMark;
        private GameObject lastActiveStage;

        [SerializeField]
        private int selectedItem = 0;

        private List<GameObject> questObjectItems = new List<GameObject>();
        private List<GameObject> questObjectStages = new List<GameObject>();
        private List<Quest> questItems = new List<Quest>();
        private List<QuestStageBase> questStages = new List<QuestStageBase>();

        QuestManager questManager;

        void Awake()
        {
            questManager = QuestManager.singleton;
        }

        public override void Open()
        {
            GameObject itemTemplate = questListContainer.transform.GetChild(0).gameObject;
            GameObject g;

            questItems = questManager.quests.FindAll(i => i.active);
            for (int i = 0; i < questItems.Count; i++)
            {
                Quest curQuest = questItems[i];

                g = Instantiate(itemTemplate, questListContainer.transform);
                g.transform.GetChild(0).GetComponent<Image>().sprite = curQuest.questIcon;
                g.transform.GetChild(1).GetComponent<TMP_Text>().text = curQuest.questName;
                g.transform.GetChild(2).GetComponent<TMP_Text>().text = curQuest.locality;

                g.GetComponent<Button>().AddEventListener(i, ItemClicked);

                questObjectItems.Add(g);
                g.SetActive(true);
            }

            UpdateStageList();
        }

        void ItemClicked(int itemIndex)
        {
            selectedItem = itemIndex;
            UpdateStageList();
        }

        private void UpdateStageList()
        {
            if (questItems.Count == 0)
            {
                return;
            }

            CleanStageList();

            Quest curQuest = questItems[selectedItem];
            GameObject itemTemplate = questStageList.transform.GetChild(0).gameObject;
            GameObject g;

            questStages = curQuest.questStages.FindAll(i => i.active || i.completed);
            for (int i = 0; i < questStages.Count; i++)
            {
                QuestStageBase curStage = questStages[i];

                g = Instantiate(itemTemplate, questStageList.transform);
                g.transform.GetChild(1).GetComponent<TMP_Text>().text = curStage.description;

                Image itemImage = g.transform.GetChild(0).GetComponent<Image>();
                if (curStage.completed)
                {
                    itemImage.sprite = correctMark;
                }
                if (curStage.active)
                {
                    if (lastActiveStage != null)
                    {
                        lastActiveStage.transform.GetChild(0).GetComponent<Image>().sprite = emptyMark;
                    }

                    lastActiveStage = g;
                    itemImage.sprite = targetMark;
                }

                questObjectStages.Add(g);
                g.SetActive(true);
            }
        }

        private void CleanQuestList()
        {
            for (int i = 0; i < questObjectItems.Count; i++)
            {
                Destroy(questObjectItems[i]);
            }

            questObjectItems = new List<GameObject>();
            questItems = new List<Quest>();
        }

        private void CleanStageList()
        {
            for (int i = 0; i < questObjectStages.Count; i++)
            {
                Destroy(questObjectStages[i]);
            }

            questObjectStages = new List<GameObject>();
            questStages = new List<QuestStageBase>();
        }

        public override void Deinit()
        {
            CleanQuestList();
            CleanStageList();
        }
    }
}