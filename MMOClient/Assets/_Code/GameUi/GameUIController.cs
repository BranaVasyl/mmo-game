using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [Header("Ineraction")]
    public GameObject interactionUi;
    public TextMeshProUGUI interactionText;

    public static GameUIController singleton;
    void Awake()
    {
        singleton = this;
    }
}
