using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV{

public class fromTest : MonoBehaviour
{
    public string parameter;

    private void OnTriggerEnter(Collider other)
    {
        FindObjectOfType<QuestManager>().setQuestOrPartComplated(parameter);
        Destroy(this.gameObject);
    }
}
}