using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class EvidenceInteraction : MonoBehaviour, IInteractable
    {
        public string GetDescription()
        {
            return "Oглянути сліди";
        }

        public void Interact(GameObject player)
        {
            gameObject.transform.parent.gameObject.GetComponent<ReshearchArea>().UpdateAreaEvidence(gameObject);
            Destroy(GetComponent<EvidenceInteraction>());
        }
    }
}
