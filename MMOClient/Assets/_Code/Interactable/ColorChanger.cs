using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class ColorChanger : MonoBehaviour, IInteractable
    {
        Material mat;

        private void Start()
        {
            mat = GetComponent<MeshRenderer>().material;
        }

        public string GetDescription()
        {
            return "Зміни на рандомний колір";
        }

        public void Interact(GameObject player)
        {
            mat.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}
