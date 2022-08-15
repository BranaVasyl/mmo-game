using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BV
{
    [System.Serializable]
    public class InvestiagtionArea
    {
        public string areaId;
        public int needInvestigated;
        public List<int> hintsFound;

        public bool areaStatus(int id){
            if(!hintsFound.Exists(i => i == id))
                hintsFound.Add(id);
            if(hintsFound.Count == needInvestigated)
                return true;
            return false;
        }
    }
}
