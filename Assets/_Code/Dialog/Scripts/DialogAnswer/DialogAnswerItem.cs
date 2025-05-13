using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV{

    [System.Serializable]
    public class DialogAnswerItem
    {
        public string nextPhrase;
        [TextArea(3, 10)]
        public string sentence;
    }
}
