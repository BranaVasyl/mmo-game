using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV{

[System.Serializable]
    public class QuestPart
    {
        public string partId;
        public string partDescription;
        public bool partCompleted;

        public bool isCompleted(){
            return partCompleted;
        }

        public void setCompleted(){
            partCompleted = true;
        }
    }
}