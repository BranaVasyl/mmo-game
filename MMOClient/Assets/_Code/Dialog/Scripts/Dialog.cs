using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{

    [System.Serializable]
    public class Dialog
    {
        public string dialogName;
        public string entryPointGUID;
        public string startPhrase;

        public DialogPhrase[] allPhrase;
        public DialogAnswer[] allAnswer;
        public List<DialogOutput> allOutputNode = new List<DialogOutput>();
        public List<DialogSelected> allSelectedNode = new List<DialogSelected>();
        public List<DialogTrade> allTradeNode = new List<DialogTrade>();
    }
}
