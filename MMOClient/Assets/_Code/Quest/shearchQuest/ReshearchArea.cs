using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class ReshearchArea : MonoBehaviour
    {
        public string areaId;
        public List<GameObject> needInvestigated;
        public List<QuestEvent> completedActions;
        public bool removeAferUse = true;

        public void UpdateAreaEvidence(GameObject go)
        {
            int index = needInvestigated.IndexOf(go);
            if (index < 0)
            {
                return;
            }

            needInvestigated.RemoveAt(index);
            if (needInvestigated.Count == 0)
            {
                OnComplete();
            }
        }

        public void OnComplete()
        {
            for (int i = 0; i < completedActions.Count; i++)
            {
                completedActions[i].TriggerEvent();
            }

            if (removeAferUse)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
