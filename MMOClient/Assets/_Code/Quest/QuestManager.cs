using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BV{
    
    public class QuestManager : MonoBehaviour
    {
        public List<Quest> quest;
        public List<InvestiagtionArea> InvestiagtionAreas; 

        public Quest getQuest(string id)
        {
            Quest resultQuest = quest.Find(i => i.questId == id);
            
            if(resultQuest != null) {
                return resultQuest;
            }

            return null;
        }

        public void setQuestOrPartComplated(string options){
            string[] command = options.Split(new char[]{'&'});

            Quest quest = getQuest(command[0]);
            if(quest != null){
                if(command.Length > 1){
                    quest.setPartCompleted(command[1]);
                }
                else {
                    quest.setActive();
                }
            }
        }

        public bool getQuestOrPartComplated(string options){
            string[] command = options.Split(new char[]{'&'});

            Quest quest = getQuest(command[0]);
            if(quest != null){
                if(command.Length > 1)
                    return quest.isPartCompleted(command[1]);
                else
                    return quest.isQuestActive();
            }

            return false;
        }

        public void UpdateAreaEvidence(string areaId, int evidenceId, int needInvestigated, string options){
            if(getQuestOrPartComplated(options)){
                return;
            }
            
            InvestiagtionArea area = InvestiagtionAreas.Find(e => e.areaId == areaId);
            if(area == null){
                if(needInvestigated == 1){
                    setQuestOrPartComplated(options);
                    return;
                }
                InvestiagtionAreas.Add(new InvestiagtionArea{areaId = areaId, needInvestigated = needInvestigated, hintsFound = new List<int>(){evidenceId}});
            }else
            {
                if(area.areaStatus(evidenceId)){
                    setQuestOrPartComplated(options);
                    InvestiagtionAreas.Remove(area);
                }
            }
            
        }
    }
}
