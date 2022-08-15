using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class ReshearchArea : MonoBehaviour
    {
        public string areaId;
        public string partQuestId;
        public int needInvestigated;

        public void EvidenceInvestigated(int id){
            FindObjectOfType<QuestManager>().UpdateAreaEvidence(areaId, id, needInvestigated, partQuestId);
        }
        
    }
}
