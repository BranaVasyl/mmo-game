using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV{

[System.Serializable]
    public class DialogSelected
    {
        public string nodeId;
        public Vector2 nodePosition;
        public string parameter;
        public string nextElementPositive;
        public string nextElementNegative;
    }
}
