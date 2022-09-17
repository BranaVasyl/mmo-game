using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/Spell", order = 3)]
    public class Spell : ItemData
    {
        [Header("Spell Stats")]
        public SpellType spellType;
        public GameObject projectile;
        public GameObject particlePrefab;
    }
}
