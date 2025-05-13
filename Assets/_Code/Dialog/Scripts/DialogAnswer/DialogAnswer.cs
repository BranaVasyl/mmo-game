using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV{

    [System.Serializable]
    public class DialogAnswer
    {
        public string idAnswer;
        public string nodeText;
        public Vector2 nodePosition;
        public List<DialogAnswerItem> answerItems = new List<DialogAnswerItem>();

    }
}
