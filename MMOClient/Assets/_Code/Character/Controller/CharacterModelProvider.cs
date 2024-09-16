using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class CharacterModelProvider : MonoBehaviour
    {
        public GameObject rightHandPivot;
        public GameObject leftHandPivot;

        public GameObject GetRightHandPivot() {
            return rightHandPivot;
        }
        
        public GameObject GetLeftHandPivot() {
            return leftHandPivot;
        }
    }
}
