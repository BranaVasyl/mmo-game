using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BV
{
    [Serializable]
    public class EnemyData
    {
        public string id;
        public Vector3 position;
        public Quaternion rotation;
        public float vertical;
        public float horizontal;
        public bool isDead;
        public bool move;
        public bool isInteracting;
        public bool run;
        public bool walk;
        public bool isTwoHanded;
        public string currentAnimation;
        public string tempAnimationId;
        public string playerSpawnedId;
        public bool isControlling = false;
    }
}
