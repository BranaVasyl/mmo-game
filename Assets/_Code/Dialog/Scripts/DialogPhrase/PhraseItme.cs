using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV{

    [System.Serializable]
    public class DialogPhraseItem
    {
        public string id;
        public string speaker;
        public float showTime = 2f;

        [TextArea(3, 10)]
        public string sentence;
    }
}