using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV{

    [System.Serializable]
    public class DialogPhrase
    {
        public string idPhrase;
        public string nodeText; //unique name
        public Vector2 nodePosition;
        public string nextAnswer;
        public List<DialogPhraseItem> phraseItems = new List<DialogPhraseItem>();
    }
}