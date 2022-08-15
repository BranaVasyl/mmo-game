    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class Evidence : MonoBehaviour{
            public int id;

            private void OnTriggerEnter(Collider other){
                Debug.Log("Evidence");    
                gameObject.transform.parent.gameObject.GetComponent<ReshearchArea>().EvidenceInvestigated(id);
                gameObject.GetComponent<Evidence>().enabled = false;
            }
        }
    
}