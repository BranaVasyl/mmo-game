using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/Game Item/Spell", order = 3)]
    public class Spell : ItemData
    {
        [Header("Spell Stats")]
        public SpellType spellType;
        public SpellClass spellClass;
        public List<SpellAction> actions = new List<SpellAction>(); 
        public GameObject projectile;
        public GameObject particlePrefab;
    }
}
