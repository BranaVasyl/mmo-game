using System.Collections.Generic;
using System.Collections;
using Project.Utility;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;

namespace BV
{
    public class JournalController : MenuPanel
    {
        [SerializeField]
        private GameObject questListContainer;
        [SerializeField]
        private GameObject questStageList;

        [Header("Quest icons")]
        public Sprite emptyMark;
        public Sprite correctMark;
        public Sprite incorrectMark;
        public Sprite targetMark;

        private GameObject lastActiveStage;
        private int selectedItem = 0;

        private List<GameObject> questObjectItems = new List<GameObject>();
        private List<GameObject> questObjectStages = new List<GameObject>();

        private List<Quest> questItems = new List<Quest>();
        private List<Quest> activeQusets = new List<Quest>();
        private List<Quest> comletedQusets = new List<Quest>();

        private List<QuestStageBase> questStages = new List<QuestStageBase>();

        QuestManager questManager;

        void Awake() 
        {
            questManager = QuestManager.Instance;
        }

        public override void Open()
        {
            activeQusets = questManager.quests.FindAll(i => i.active);
            questItems = questItems.Concat(activeQusets).ToList();
            InitQuestList(activeQusets);

            comletedQusets = questManager.quests.FindAll(i => i.completed);
            questItems = questItems.Concat(comletedQusets).ToList();
            InitQuestList(comletedQusets);

            for (int i = 0; i < questObjectItems.Count; i++)
            {
                questObjectItems[i].GetComponent<Button>().AddEventListener(i, ItemClicked);
            }

            UpdateStageList();
        }

        void InitQuestList(List<Quest> questList)
        {
            GameObject itemTemplate = questListContainer.transform.GetChild(0).gameObject;
            GameObject g;

            for (int i = 0; i < questList.Count; i++)
            {
                Quest curQuest = questList[i];

                g = Instantiate(itemTemplate, questListContainer.transform);
                g.transform.GetChild(0).GetComponent<Image>().sprite = curQuest.questIcon;
                g.transform.GetChild(1).GetComponent<TMP_Text>().text = curQuest.questName;
                g.transform.GetChild(2).GetComponent<TMP_Text>().text = curQuest.locality;
                g.transform.GetChild(3).gameObject.SetActive(curQuest.completed);

                questObjectItems.Add(g);
                g.SetActive(true);
            }
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
                if (curStage.active && !curQuest.completed)
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

        public override void Close()
        {
            CleanQuestList();
            CleanStageList();
        }

        public override void Deinit()
        {
            Close();
        }
    }
}